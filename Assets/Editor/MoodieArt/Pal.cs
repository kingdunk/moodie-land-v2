using UnityEngine;

namespace MoodieLand.EditorTools
{
    /// <summary>Shared pastel palette. Every sprite pulls from here so the art stays consistent.</summary>
    public static class Pal
    {
        public static readonly Color32 Ink = Canvas2D.Hex("4A4038");
        public static readonly Color32 InkSoft = Canvas2D.Hex("77685C");
        public static readonly Color32 White = Canvas2D.Hex("FFFDF8");
        public static readonly Color32 Cream = Canvas2D.Hex("FFF6E9");
        public static readonly Color32 Paper = Canvas2D.Hex("FFFCF6");
        public static readonly Color32 Teal = Canvas2D.Hex("399C91");
        public static readonly Color32 TealDark = Canvas2D.Hex("2C7A72");
        public static readonly Color32 TealLight = Canvas2D.Hex("BFE8E2");
        public static readonly Color32 Peach = Canvas2D.Hex("FFC7A8");
        public static readonly Color32 PeachDark = Canvas2D.Hex("F2A582");
        public static readonly Color32 Pink = Canvas2D.Hex("F7A8B2");
        public static readonly Color32 PinkLight = Canvas2D.Hex("FCD3D8");
        public static readonly Color32 Lav = Canvas2D.Hex("C7CEEA");
        public static readonly Color32 LavDark = Canvas2D.Hex("A9B2D8");
        public static readonly Color32 Mint = Canvas2D.Hex("A8E6CF");
        public static readonly Color32 MintDark = Canvas2D.Hex("86CDB4");
        public static readonly Color32 Sun = Canvas2D.Hex("FFE08A");
        public static readonly Color32 SunDark = Canvas2D.Hex("F0C25E");
        public static readonly Color32 Sky = Canvas2D.Hex("DCF0F6");
        public static readonly Color32 SkyDeep = Canvas2D.Hex("B9E0F0");
        public static readonly Color32 SkyWarm = Canvas2D.Hex("FFEFE2");
        public static readonly Color32 Grass = Canvas2D.Hex("B7DFA4");
        public static readonly Color32 GrassDark = Canvas2D.Hex("98CB88");
        public static readonly Color32 GrassDeep = Canvas2D.Hex("7FB673");
        public static readonly Color32 Brown = Canvas2D.Hex("B08968");
        public static readonly Color32 BrownDark = Canvas2D.Hex("8A6A4F");
        public static readonly Color32 Orange = Canvas2D.Hex("F0A05A");
        public static readonly Color32 OrangeDark = Canvas2D.Hex("D98643");
        public static readonly Color32 Squirrel = Canvas2D.Hex("C08E63");
        public static readonly Color32 SquirrelDark = Canvas2D.Hex("A1704A");
        public static readonly Color32 Blue = Canvas2D.Hex("8FB8E8");
        public static readonly Color32 BlueDark = Canvas2D.Hex("6F98C9");
        public static readonly Color32 Cloud = Canvas2D.Hex("FFFDFB");
        public static readonly Color32 CloudShade = Canvas2D.Hex("E9F1F6");
        public static readonly Color32 Blush = Canvas2D.Hex("F79FA8", 0.55f);
        public static readonly Color32 Shadow = Canvas2D.Hex("5C4C43", 0.22f);
    }

    /// <summary>One character's colouring and silhouette rules.</summary>
    public class CharStyle
    {
        public string key;
        public Color32 fur;
        public Color32 furDark;
        public Color32 belly;
        public Color32 earInner;
        public Color32 accessory;
        public Color32 accessory2;
        public string ears = "round";     // fox | round | bunny | spike
        public string outfit = "none";    // scarf | overalls | flower | none
        public float headScale = 1f;
    }
}
