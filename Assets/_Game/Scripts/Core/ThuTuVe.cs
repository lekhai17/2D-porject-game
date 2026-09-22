using UnityEngine;

namespace KingdomRuins
{
    /// <summary>
    /// Thu tu ve cho moi vat the dung tren mat dat (nha, cay, nguoi, do trang tri).
    /// Cang o thap (y nho) cang ve sau de che vat phia tren.
    /// Luon lon hon moi lop dia hinh (dia hinh dung so am), du ban do cao bao nhieu.
    /// </summary>
    public static class ThuTuVe
    {
        public const int Goc = 2000;       // y = 0
        public const int MoiO = 20;        // 20 bac moi o, du min de phan biet vat sat nhau
        public const int TrenCung = 32000; // chu noi, hieu ung can hien tren tat ca

        public static int TheoY(float y, int lech = 0) => Goc - Mathf.RoundToInt(y * MoiO) + lech;
    }
}
