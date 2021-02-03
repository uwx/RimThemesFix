using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using aRandomKiwi.RimThemes;
using HarmonyLib;
using UnityEngine;
using Verse;

// ReSharper disable InconsistentNaming

namespace RimThemesFix
{
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        private static readonly Dictionary<string, Color> DBTextColorWhite;
        private static readonly Dictionary<string, Color> DBTextColorYellow;
        private static readonly Dictionary<string, Color> DBTextColorGreen;
        private static readonly Dictionary<string, Color> DBTextColorRed;
        private static readonly Dictionary<string, Color> DBTextColorCyan;
        private static readonly Dictionary<string, Color> DBTextColorBlue;
        private static readonly Dictionary<string, Color> DBTextColorGray;
        private static readonly Dictionary<string, Color> DBTextColorMagenta;
        private static readonly Dictionary<string, Color> DBTexColorWhite;
        private static readonly Dictionary<string, Color> DBTexColorYellow;
        private static readonly Dictionary<string, Color> DBTexColorGreen;
        private static readonly Dictionary<string, Color> DBTexColorRed;
        private static readonly Dictionary<string, Color> DBTexColorCyan;
        private static readonly Dictionary<string, Color> DBTexColorBlue;
        private static readonly Dictionary<string, Color> DBTexColorGray;
        private static readonly Dictionary<string, Color> DBTexColorMagenta;

        private static readonly Dictionary<string, Dictionary<Color, Color>> DBDynColor;
        
        private static readonly Dictionary<string, Dictionary<string, string>> DBText;
        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, Color>>> DBColor;
        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, Texture2D>>> DBTex;
        private static readonly string MiscKey;
        private static readonly string VanillaThemeID;

        private static readonly AccessTools.FieldRef<bool> tempDisableDynColor;
        private static readonly AccessTools.FieldRef<string> curTheme;

        // this static constructor runs to create a HarmonyInstance and install a patch.
        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.uwx.rimthemesfix");
            harmony.PatchAll(typeof(HarmonyPatches).Assembly);

            var rimThemesAssembly = typeof(GameFinalizeInitPatch).Assembly;

            var colorsSubstitutionType = rimThemesAssembly.GetType("aRandomKiwi.RimThemes.ColorsSubstitution");
            var themesType = rimThemesAssembly.GetType("aRandomKiwi.RimThemes.Themes");
            var utilsType = rimThemesAssembly.GetType("aRandomKiwi.RimThemes.Utils");
            var settingsType = rimThemesAssembly.GetType("aRandomKiwi.RimThemes.Settings");

            var getTextSubstitutionColorOriginal = AccessTools.Method(colorsSubstitutionType, nameof(getTextSubstitutionColor));
            var getTextureSubstitutionColorOriginal = AccessTools.Method(colorsSubstitutionType, nameof(getTextureSubstitutionColor));
            var getTextOriginal = AccessTools.Method(themesType, nameof(getText));
            var getThemeTexOriginal = AccessTools.Method(themesType, nameof(getThemeTex));
            var getColorMiscOriginal = AccessTools.Method(themesType, nameof(getColorMisc));

            var getTextSubstitutionColorReplacement = new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(getTextSubstitutionColor)));
            var getTextureSubstitutionColorReplacement = new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(getTextureSubstitutionColor)));
            var getTextReplacement = new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(getText)));
            var getThemeTexReplacement = new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(getThemeTex)));
            var getColorMiscReplacement = new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), nameof(getColorMisc)));

            static T GetField<T>(Type type, string name)
            {
                return (T) type.GetField(name, AccessTools.allDeclared).GetValue(null);
            }
            
            DBTextColorWhite = GetField<Dictionary<string, Color>>(themesType, nameof(DBTextColorWhite));
            DBTextColorYellow = GetField<Dictionary<string, Color>>(themesType, nameof(DBTextColorYellow));
            DBTextColorGreen = GetField<Dictionary<string, Color>>(themesType, nameof(DBTextColorGreen));
            DBTextColorRed = GetField<Dictionary<string, Color>>(themesType, nameof(DBTextColorRed));
            DBTextColorCyan = GetField<Dictionary<string, Color>>(themesType, nameof(DBTextColorCyan));
            DBTextColorBlue = GetField<Dictionary<string, Color>>(themesType, nameof(DBTextColorBlue));
            DBTextColorGray = GetField<Dictionary<string, Color>>(themesType, nameof(DBTextColorGray));
            DBTextColorMagenta = GetField<Dictionary<string, Color>>(themesType, nameof(DBTextColorMagenta));
            DBTexColorWhite = GetField<Dictionary<string, Color>>(themesType, nameof(DBTexColorWhite));
            DBTexColorYellow = GetField<Dictionary<string, Color>>(themesType, nameof(DBTexColorYellow));
            DBTexColorGreen = GetField<Dictionary<string, Color>>(themesType, nameof(DBTexColorGreen));
            DBTexColorRed = GetField<Dictionary<string, Color>>(themesType, nameof(DBTexColorRed));
            DBTexColorCyan = GetField<Dictionary<string, Color>>(themesType, nameof(DBTexColorCyan));
            DBTexColorBlue = GetField<Dictionary<string, Color>>(themesType, nameof(DBTexColorBlue));
            DBTexColorGray = GetField<Dictionary<string, Color>>(themesType, nameof(DBTexColorGray));
            DBTexColorMagenta = GetField<Dictionary<string, Color>>(themesType, nameof(DBTexColorMagenta));
            DBDynColor = GetField<Dictionary<string, Dictionary<Color, Color>>>(themesType, nameof(DBDynColor));
            DBText = GetField<Dictionary<string, Dictionary<string, string>>>(themesType, nameof(DBText));
            DBColor = GetField<Dictionary<string, Dictionary<string, Dictionary<string, Color>>>>(themesType, nameof(DBColor));
            DBTex = GetField<Dictionary<string, Dictionary<string, Dictionary<string, Texture2D>>>>(themesType, nameof(DBTex));
            MiscKey = GetField<string>(themesType, nameof(MiscKey));
            VanillaThemeID = GetField<string>(themesType, nameof(VanillaThemeID));
            
            tempDisableDynColor = AccessTools.StaticFieldRefAccess<bool>(AccessTools.Field(utilsType, nameof(tempDisableDynColor)));
            curTheme = AccessTools.StaticFieldRefAccess<string>(AccessTools.Field(settingsType, nameof(curTheme)));

            harmony.Patch(getTextSubstitutionColorOriginal, getTextSubstitutionColorReplacement);
            harmony.Patch(getTextureSubstitutionColorOriginal, getTextureSubstitutionColorReplacement);
            harmony.Patch(getTextOriginal, getTextReplacement);
            harmony.Patch(getThemeTexOriginal, getThemeTexReplacement);
            harmony.Patch(getColorMiscOriginal, getColorMiscReplacement);
        }

        private static string lastCurTheme;
        private static (Color from, Color to)[] curThemeColorOptions;
        private static (Color from, Color to)[] curThemeTextColorOptions;
        private static Dictionary<Color, Color> curThemeColorRemappings;
        private static Dictionary<string, string> curThemeTextReplacements;
        private static Dictionary<string, Color> curThemeMiscColorReplacements;
        private static Dictionary<string, Dictionary<string, Texture2D>> curThemeTexReplacements;
        private static Dictionary<string, Dictionary<string, Texture2D>> vanillaThemeTexReplacements;

        // ReSharper disable once RedundantAssignment
        public static bool getTextSubstitutionColor(Color color, ref Color __result)
        {
            if (tempDisableDynColor())
            {
                __result = color;
                return false;
            }

            var curTheme = HarmonyPatches.curTheme();

            if (!ReferenceEquals(lastCurTheme, curTheme))
            {
                LoadNewTheme(curTheme);
            }

            foreach (var (from, to) in curThemeTextColorOptions)
            {
                if (CloseEnough(color, from))
                {
                    __result = to;
                    __result.a = color.a;
                    return false;
                }
            }

            if (curThemeColorRemappings != null && curThemeColorRemappings.TryGetValue(color, out __result))
            {
                return false;
            }

            __result = color;
            return false;
        }

        // ReSharper disable once RedundantAssignment
        public static bool getTextureSubstitutionColor(Color color, ref Color __result)
        {
            if (tempDisableDynColor())
            {
                __result = color;
                return false;
            }

            var curTheme = HarmonyPatches.curTheme();

            if (!ReferenceEquals(lastCurTheme, curTheme))
            {
                LoadNewTheme(curTheme);
            }

            foreach (var (from, to) in curThemeColorOptions)
            {
                if (CloseEnough(color, from))
                {
                    __result = to;
                    __result.a = color.a;
                    return false;
                }
            }

            if (curThemeColorRemappings != null && curThemeColorRemappings.TryGetValue(color, out __result))
            {
                return false;
            }

            __result = color;
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // Not meant to be called frequently
        private static void LoadNewTheme(string curTheme)
        {
            lastCurTheme = curTheme;

            // Texture color remapping
            var curThemeColorOptionsTemp = new (Color from, Color to)[8];
            var lastI = 0;

            if (DBTexColorWhite.TryGetValue(curTheme, out var n)) curThemeColorOptionsTemp[lastI++] = (Color.white, n);
            if (DBTexColorYellow.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.yellow, n);
            if (DBTexColorRed.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.red, n);
            if (DBTexColorGreen.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.green, n);
            if (DBTexColorBlue.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.blue, n);
            if (DBTexColorCyan.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.cyan, n);
            if (DBTexColorGray.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.gray, n);
            if (DBTexColorMagenta.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.magenta, n);

            Array.Resize(ref curThemeColorOptionsTemp, lastI);
            curThemeColorOptions = curThemeColorOptionsTemp;
            
            // Text color remapping
            curThemeColorOptionsTemp = new (Color from, Color to)[8];
            lastI = 0;

            if (DBTextColorWhite.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.white, n);
            if (DBTextColorYellow.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.yellow, n);
            if (DBTextColorRed.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.red, n);
            if (DBTextColorGreen.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.green, n);
            if (DBTextColorBlue.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.blue, n);
            if (DBTextColorCyan.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.cyan, n);
            if (DBTextColorGray.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.gray, n);
            if (DBTextColorMagenta.TryGetValue(curTheme, out n)) curThemeColorOptionsTemp[lastI++] = (Color.magenta, n);

            Array.Resize(ref curThemeColorOptionsTemp, lastI);
            curThemeTextColorOptions = curThemeColorOptionsTemp;

            // Straight color remapping
            curThemeColorRemappings = DBDynColor.TryGetValue(curTheme, out var dict) ? dict : null;
            
            // Other replacements
            curThemeTextReplacements = DBText.TryGetValue(curTheme, out var dict1) ? dict1 : null;
            curThemeMiscColorReplacements = DBColor.TryGetValue(curTheme, out var dict2) ? dict2.TryGetValue(MiscKey, out var dict2_1) ? dict2_1 : null : null;
            curThemeTexReplacements = DBTex.TryGetValue(curTheme, out var dict3) ? dict3 : null;
            vanillaThemeTexReplacements = DBTex.TryGetValue(VanillaThemeID, out var dict4) ? dict4 : null;
        }

        // Fast replacement for Mathf.Approximately
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CloseEnough(Color left, Color right)
        {
            return Math.Abs(left.r - right.r) <= 0.00390625F
                   && Math.Abs(left.g - right.g) <= 0.00390625F
                   && Math.Abs(left.b - right.b) <= 0.00390625F;
        }
        
        // ReSharper disable once RedundantAssignment
        private static bool getText(string text, string dtheme, ref string __result)
        {
            if (string.IsNullOrEmpty(dtheme) || dtheme == curTheme())
            {
                if (curThemeTextReplacements != null && curThemeTextReplacements.TryGetValue(text, out var replacement))
                {
                    __result = replacement;
                    return false;
                }
            }
            else
            {
                if (DBText.TryGetValue(dtheme, out var replacements) && replacements != null && replacements.TryGetValue(text, out var replacement))
                {
                    __result = replacement;
                    return false;
                }
            }

            __result = null;
            return false;
        }
        
        // ReSharper disable once RedundantAssignment
        public static bool getThemeTex(string className, string fieldName, string dtheme, ref Texture2D __result)
        {
            if (string.IsNullOrEmpty(dtheme) || dtheme == curTheme())
            {
                if (curThemeTexReplacements != null
                    && curThemeTexReplacements.TryGetValue(className, out var classGroup)
                    && classGroup.TryGetValue(fieldName, out var replacement)
                    && replacement != null)
                {
                    __result = replacement;
                    return false;
                }
            }
            else
            {
                if (DBTex.TryGetValue(dtheme, out var replacementGroups)
                    && replacementGroups.TryGetValue(className, out var classGroup)
                    && classGroup.TryGetValue(fieldName, out var replacement)
                    && replacement != null)
                {
                    __result = replacement;
                    return false;
                }
            }

            {
                if (vanillaThemeTexReplacements != null
                    && vanillaThemeTexReplacements.TryGetValue(className, out var classGroup)
                    && classGroup.TryGetValue(fieldName, out var replacement))
                {
                    __result = replacement;
                    return false;
                }
            }

            __result = null;
            return false;
        }
        
        // ReSharper disable once RedundantAssignment
        public static bool getColorMisc(string text, string dtheme, ref Color __result)
        {
            if (string.IsNullOrEmpty(dtheme) || dtheme == curTheme())
            {
                if (curThemeMiscColorReplacements != null && curThemeMiscColorReplacements.TryGetValue(text, out var replacement))
                {
                    __result = replacement;
                    return false;
                }
            }
            else
            {
                if (DBColor.TryGetValue(dtheme, out var replacementGroups)
                    && replacementGroups.TryGetValue(MiscKey, out var replacements)
                    && replacements.TryGetValue(text, out var replacement))
                {
                    __result = replacement;
                    return false;
                }
            }
            
            __result = Color.black;
            return false;
        }
    }
}