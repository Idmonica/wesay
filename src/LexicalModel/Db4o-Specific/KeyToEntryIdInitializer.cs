using System;
using System.Collections.Generic;
using Db4objects.Db4o;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Query;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Language;

namespace WeSay.LexicalModel.Db4o_Specific
{
    public class KeyToEntryIdInitializer
    {
        static public List<KeyValuePair<string, long>> GetLexicalFormToEntryIdPairs(Db4oDataSource db4oData, string writingSystemId)
        {
            IExtObjectContainer database = db4oData.Data.Ext();
            
            List<Type> OriginalList = Db4oLexModelHelper.Singleton.DoNotActivateTypes;
            Db4oLexModelHelper.Singleton.DoNotActivateTypes = new List<Type>();
            Db4oLexModelHelper.Singleton.DoNotActivateTypes.Add(typeof(LexEntry));

            IQuery query = database.Query();
            query.Constrain(typeof(LexicalFormMultiText));
            IObjectSet lexicalForms = query.Execute();

            List<KeyValuePair<string, long>> result = new List<KeyValuePair<string, long>>();
            
            foreach (LexicalFormMultiText lexicalForm in lexicalForms)
            {
                query = database.Query();
                query.Constrain(typeof(LexEntry));
                query.Descend("_lexicalForm").Constrain(lexicalForm).Identity();
                long[] ids = query.Execute().Ext().GetIDs();

                //// If LexEntry does not cascade delete its lexicalForm then we could have a case where we
                //// don't have a entry associated with this lexicalForm.
                if (ids.Length == 0)
                {
                    continue;
                }

                result.Add(new KeyValuePair<string, long>(lexicalForm.GetBestAlternative(writingSystemId, "*"), ids[0]));
            }
            
            Db4oLexModelHelper.Singleton.DoNotActivateTypes = OriginalList;

            return result;
        }

        static public List<KeyValuePair<string, long>> GetGlossToEntryIdPairs(Db4oDataSource db4oData, string writingSystemId)
        {
            IExtObjectContainer database = db4oData.Data.Ext();

            List<Type> OriginalList = Db4oLexModelHelper.Singleton.DoNotActivateTypes;
            Db4oLexModelHelper.Singleton.DoNotActivateTypes = new List<Type>();
            Db4oLexModelHelper.Singleton.DoNotActivateTypes.Add(typeof(LexEntry));
            Db4oLexModelHelper.Singleton.DoNotActivateTypes.Add(typeof(LexSense));

            IQuery query = database.Query();
            query.Constrain(typeof(SenseGlossMultiText));
            IObjectSet glosses = query.Execute();

            List<KeyValuePair<string, long>> result = new List<KeyValuePair<string, long>>();

            foreach (SenseGlossMultiText gloss in glosses)
            {
                IQuery senseQuery = database.Query();
                senseQuery.Constrain(typeof (LexSense));
                senseQuery.Descend("_gloss").Constrain(gloss).Identity();

                query = senseQuery.Descend("_parent");
                
                long[] ids = query.Execute().Ext().GetIDs();

                if (ids.Length == 0)
                {
                    continue;
                }


                foreach (string s in SplitGlossAtSemicolon(gloss, writingSystemId))
                {
                    result.Add(new KeyValuePair<string, long>(s, ids[0]));
                }
            }

            Db4oLexModelHelper.Singleton.DoNotActivateTypes = OriginalList;

            return result;
        }

        public static IEnumerable<string> SplitGlossAtSemicolon(MultiText gloss, string writingSystemId)
        {
            bool exact = true;
            string glossText = gloss.GetExactAlternative(writingSystemId);
            if (glossText == string.Empty)
            {
                exact = false;
                glossText = gloss.GetBestAlternative(writingSystemId, "*");
            }

            List<string> result = new List<string>();
            string[] keylist = glossText.Split(new char[] { ';' });
            for (int i = 0; i < keylist.Length; i++)
            {
                string k = keylist[i].Trim();

                if (exact || i == keylist.Length - 1)
                {
                    result.Add(k);
                }
                else
                {
                    result.Add(k + "*");
                }
            }
            return result;
        }

        public delegate IEnumerable<string> GetKeys(LexEntry entry);

        public static List<KeyValuePair<string, long>> GetKeyToEntryIdPairs(Db4oDataSource db4oData, GetKeys getKeys)
        {
            IExtObjectContainer database = db4oData.Data.Ext();

            IQuery query = database.Query();
            query.Constrain(typeof(LexEntry));
            IObjectSet entries = query.Execute();

            List<KeyValuePair<string, long>> result = new List<KeyValuePair<string, long>>();

            foreach (LexEntry entry in entries)
            {
                foreach (string key in getKeys(entry))
                {
                    result.Add(new KeyValuePair<string, long>(key, database.GetID(entry)));
                }
            }

            return result;
        }
    }
}
