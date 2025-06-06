using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGSC;

namespace QM_EnemyCountIndicator
{
    internal class Locale
    {
        Dictionary<Localization.Lang, Dictionary<string, string>> customLangDict = new Dictionary<Localization.Lang, Dictionary<string, string>>();

        internal Locale()
        {
            customLangDict.Add(Localization.Lang.EnglishUS, new Dictionary<string, string>());
            customLangDict.Add(Localization.Lang.Russian, new Dictionary<string, string>());
            customLangDict.Add(Localization.Lang.German, new Dictionary<string, string>());
            customLangDict.Add(Localization.Lang.French, new Dictionary<string, string>());
            customLangDict.Add(Localization.Lang.Spanish, new Dictionary<string, string>());
            customLangDict.Add(Localization.Lang.Polish, new Dictionary<string, string>());
            customLangDict.Add(Localization.Lang.Turkish, new Dictionary<string, string>());
            customLangDict.Add(Localization.Lang.BrazilianPortugal, new Dictionary<string, string>());
            customLangDict.Add(Localization.Lang.Korean, new Dictionary<string, string>());
            customLangDict.Add(Localization.Lang.Japanese, new Dictionary<string, string>());
            customLangDict.Add(Localization.Lang.ChineseSimp, new Dictionary<string, string>());

            customLangDict[Localization.Lang.EnglishUS].Add("enemy.indicator.lang", "ENEMIES");
            customLangDict[Localization.Lang.Russian].Add("enemy.indicator.lang", "ВРАГИ");
            customLangDict[Localization.Lang.German].Add("enemy.indicator.lang", "FEINDE");
            customLangDict[Localization.Lang.French].Add("enemy.indicator.lang", "ENNEMIS");
            customLangDict[Localization.Lang.Spanish].Add("enemy.indicator.lang", "ENEMIGOS");
            customLangDict[Localization.Lang.Polish].Add("enemy.indicator.lang", "PRZECIWNICY");
            customLangDict[Localization.Lang.Turkish].Add("enemy.indicator.lang", "DÜŞMANLAR");
            customLangDict[Localization.Lang.BrazilianPortugal].Add("enemy.indicator.lang", "INIMIGOS");
            customLangDict[Localization.Lang.Korean].Add("enemy.indicator.lang", "적들");
            customLangDict[Localization.Lang.Japanese].Add("enemy.indicator.lang", "敵 ");
            customLangDict[Localization.Lang.ChineseSimp].Add("enemy.indicator.lang", "敌人");
        }

        public string GetString(string key, Localization.Lang lang)
        {

            if (customLangDict[lang].ContainsKey(key))
            {

                return customLangDict[lang][key];
            }
            else
            {

                return key;
            }
        }
    }
}
