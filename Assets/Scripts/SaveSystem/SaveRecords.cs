using SQLite;

[Table("SaveSlots")]
public class SaveSlotRecord
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("slot_name")]
    public string SlotName { get; set; }

    [Column("created_at")]
    public string CreatedAt { get; set; }

    [Column("updated_at"), Indexed]
    public string UpdatedAt { get; set; }

    [Column("current_scene")]
    public string CurrentScene { get; set; }

    [Column("player_x")]
    public float PlayerX { get; set; }

    [Column("player_y")]
    public float PlayerY { get; set; }

    [Column("play_time_seconds")]
    public double PlayTimeSeconds { get; set; }
}

[Table("QuestStates")]
public class QuestStateRecord
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("save_slot_id"), Indexed]
    public int SaveSlotId { get; set; }

    [Column("quest_key")]
    public string QuestKey { get; set; }

    [Column("state")]
    public int State { get; set; }

    [Column("updated_at")]
    public string UpdatedAt { get; set; }
}

[Table("InventoryItems")]
public class InventoryItemRecord
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("save_slot_id"), Indexed]
    public int SaveSlotId { get; set; }

    [Column("item_key")]
    public string ItemKey { get; set; }

    [Column("item_type")]
    public string ItemType { get; set; }

    [Column("amount")]
    public int Amount { get; set; }

    [Column("is_collected")]
    public int IsCollected { get; set; }
}

[Table("Memories")]
public class MemoryRecord
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("save_slot_id"), Indexed]
    public int SaveSlotId { get; set; }

    [Column("memory_key")]
    public string MemoryKey { get; set; }

    [Column("title")]
    public string Title { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("photo_asset_key")]
    public string PhotoAssetKey { get; set; }

    [Column("is_unlocked")]
    public int IsUnlocked { get; set; }
}

public class GameSaveSnapshot
{
    public bool IsQuestStarted;
    public bool IsLockedDoorTried;
    public bool IsPuzzleSolved;
    public bool AreCoinsHandedIn;
    public bool IsFinalPathOpen;
    public int Coins;
    public System.Collections.Generic.List<string> Items = new System.Collections.Generic.List<string>();
    public System.Collections.Generic.List<string> KeyIds = new System.Collections.Generic.List<string>();
    public System.Collections.Generic.List<string> CollectedCoinIds = new System.Collections.Generic.List<string>();
    public System.Collections.Generic.List<string> MemoryKeys = new System.Collections.Generic.List<string>();
    public System.Collections.Generic.List<string> MemoryTitles = new System.Collections.Generic.List<string>();
}
