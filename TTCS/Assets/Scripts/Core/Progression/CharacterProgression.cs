using System;
using TTCS.Core.Save;

namespace TTCS.Core.Progression
{
    public static class CharacterProgression
    {
        public static int GetExpToNextLevel(int level)
        {
            level = Math.Max(1, level);
            return 100 + level * 50;
        }

        /// <summary>
        /// Applies gained exp to a character, leveling up as needed.
        /// SaveData stores (Level, ExpTowardsNextLevel).
        /// Returns the updated (level, expTowardsNextLevel).
        /// </summary>
        public static (int Level, int ExpTowardsNext) ApplyExp(SaveData save, string characterId, int gainedExp)
        {
            if (save == null || string.IsNullOrWhiteSpace(characterId) || gainedExp <= 0)
            {
                var existingLevel = save?.GetCharacterLevel(characterId) ?? 1;
                var existingExp = save?.GetCharacterExp(characterId) ?? 0;
                return (Math.Max(1, existingLevel), Math.Max(0, existingExp));
            }

            var level = Math.Max(1, save.GetCharacterLevel(characterId));
            var exp = Math.Max(0, save.GetCharacterExp(characterId));

            exp += gainedExp;

            // Level up loop
            // Exp is stored as progress to next level, not cumulative.
            var safety = 0;
            while (safety++ < 200)
            {
                var expToNext = Math.Max(1, GetExpToNextLevel(level));
                if (exp < expToNext)
                {
                    break;
                }

                exp -= expToNext;
                level++;
            }

            save.SetCharacterLevel(characterId, level);
            save.SetCharacterExp(characterId, exp);

            return (level, exp);
        }
    }
}
