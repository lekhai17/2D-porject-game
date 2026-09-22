namespace KingdomRuins
{
    public enum GamePhase
    {
        Kingdom,
        Battle,
        GameOver
    }

    /// <summary>
    /// Tai nguyen trong game. THU TU CU (0..4) PHAI GIU NGUYEN:
    /// cac asset ScriptableObject luu enum theo chi so, doi thu tu se lam sai het.
    /// Loai moi luon them vao CUOI danh sach.
    /// </summary>
    public enum ResourceType
    {
        Souls = 0,        // Linh hon - trieu hoi quai
        Bone = 1,         // Xuong - vat lieu
        DarkCrystal = 2,  // Tinh the hac am - nang cap
        Meat = 3,         // Thit - thuc an cho quai
        Gold = 4,         // Vang - dong xu, tieu chung

        Gem = 5,          // Ngoc - tien te cao cap
        Wood = 6,         // Go
        Stone = 7,        // Da
        Food = 8,         // Luong thuc cua dan
        Army = 9,         // Quan so
        Population = 10,  // Dan so
        Renown = 11       // Danh vong
    }

    public enum DamageType
    {
        Physical,
        Magic
    }

    public enum Faction
    {
        Dungeon,
        Invader
    }

    /// <summary>
    /// Loai dia hinh cua khu dat. Moi cong trinh chi xay duoc tren dung loai cua no,
    /// nen mo phai len nui, trai go phai vao rung.
    /// </summary>
    public enum LoaiKhuDat
    {
        DongBang = 0,
        Nui = 1,
        Rung = 2,
        VenBien = 3
    }
}
