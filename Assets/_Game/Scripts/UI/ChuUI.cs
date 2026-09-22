using UnityEngine;
using UnityEngine.UI;

namespace KingdomRuins
{
    /// <summary>Font UI dong nhat, dong goi trong build va ho tro tieng Viet.</summary>
    public static class ChuUI
    {
        private static Font thuong, dam;
        public static Font Thuong => thuong != null ? thuong : thuong = Nap("Inter-Regular");
        public static Font Dam => dam != null ? dam : dam = Nap("Inter-SemiBold");

        private static Font Nap(string ten)
        {
            var font = Resources.Load<Font>("UI/Fonts/" + ten);
            return font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        public static void ApDung(Text chu, bool inDam = false)
        {
            if (chu == null) return;
            chu.font = inDam ? Dam : Thuong;
            // Dung font Semibold that, khong gia lap Bold them mot lan nua.
            chu.fontStyle = FontStyle.Normal;
            // Outline ke thua Shadow: tat ca hai, chi tren chu, giu vien icon/panel.
            foreach (var hieuUng in chu.GetComponents<Shadow>()) hieuUng.enabled = false;
        }
    }
}
