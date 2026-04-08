using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TTCS.Meta.Inventory
{
    public readonly struct AccessoryStatBonus
    {
        public string StatKey { get; }
        public int Amount { get; }
        public string IconPath { get; }

        public AccessoryStatBonus(string statKey, int amount, string iconPath)
        {
            StatKey = statKey;
            Amount = amount;
            IconPath = iconPath;
        }
    }

    public static class AccessoryStatUtility
    {
        private static readonly Regex LegacyPattern = new Regex(@"([+-]?\d+)\s*([A-Za-z]+)", RegexOptions.Compiled);

        public static List<AccessoryStatBonus> GetBonuses(ItemDataModel item)
        {
            var results = new List<AccessoryStatBonus>();
            if (item == null)
            {
                return results;
            }

            if (item.statBonuses != null)
            {
                for (var i = 0; i < item.statBonuses.Count; i++)
                {
                    var bonus = item.statBonuses[i];
                    if (bonus == null || string.IsNullOrWhiteSpace(bonus.statKey) || bonus.amount == 0)
                    {
                        continue;
                    }

                    results.Add(new AccessoryStatBonus(NormalizeStatKey(bonus.statKey), bonus.amount, bonus.iconPath));
                }
            }

            if (results.Count > 0)
            {
                return results;
            }

            if (string.IsNullOrWhiteSpace(item.statDescription))
            {
                return results;
            }

            var matches = LegacyPattern.Matches(item.statDescription);
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (!int.TryParse(match.Groups[1].Value, out var amount))
                {
                    continue;
                }

                var statKey = NormalizeStatKey(match.Groups[2].Value);
                if (string.IsNullOrWhiteSpace(statKey) || amount == 0)
                {
                    continue;
                }

                results.Add(new AccessoryStatBonus(statKey, amount, GetDefaultIconPath(statKey)));
            }

            return results;
        }

        public static string NormalizeStatKey(string statKey)
        {
            if (string.IsNullOrWhiteSpace(statKey))
            {
                return string.Empty;
            }

            var key = statKey.Trim().ToUpperInvariant();
            if (key == "DEFENSE") return "DEF";
            if (key == "ATTACK") return "ATK";
            if (key == "SPEED") return "SPD";
            if (key == "HEALTH") return "HP";
            if (key == "CRITRATE") return "CRIT";
            return key;
        }

        public static string GetDefaultIconPath(string statKey)
        {
            switch (NormalizeStatKey(statKey))
            {
                case "HP":
                    return "UI/Element/icon_element_light";
                case "ATK":
                    return "UI/Role/icon_role_warrior";
                case "DEF":
                    return "UI/Role/icon_role_tank";
                case "SPD":
                    return "UI/Element/icon_element_lightning";
                default:
                    return string.Empty;
            }
        }

        public static string GetDisplayName(string statKey)
        {
            switch (NormalizeStatKey(statKey))
            {
                case "HP":
                    return "HP";
                case "ATK":
                    return "ATK";
                case "DEF":
                    return "DEF";
                case "SPD":
                    return "SPD";
                case "CRIT":
                    return "CRIT";
                default:
                    return NormalizeStatKey(statKey);
            }
        }
    }
}
