using System;

[Serializable]
public class PlayerSave
{
    public int stealthLevel;
    public int pickpocketLevel;
    public int distractionLevel;
    public int money;
    public JsonDateTime lastUpdateTime;

    public PlayerSave(int stealthLevel, int pickpocketLevel, int distractionLevel, int money)
    {
        this.stealthLevel = stealthLevel;
        this.distractionLevel = distractionLevel;
        this.pickpocketLevel = pickpocketLevel;
        this.money = money;
        SetLastUpdateTime();
    }

    public void SetLastUpdateTime()
    {
        lastUpdateTime = (JsonDateTime) DateTime.Now;
    }

    public string ToString()
    {
        return string.Format(
            "stealthLevel: {0}, distractionLevel: {1}, pickpocketLevel: {2}, money: {3}, lastUpdateTime: {4}",
            stealthLevel, distractionLevel, pickpocketLevel, money, lastUpdateTime.ToString());
    }
}

[Serializable]
public struct JsonDateTime {
    public long value;
    public static implicit operator DateTime(JsonDateTime jdt) {
        return DateTime.FromFileTimeUtc(jdt.value);
    }
    public static implicit operator JsonDateTime(DateTime dt) {
        JsonDateTime jdt = new JsonDateTime();
        jdt.value = dt.ToFileTimeUtc();
        return jdt;
    }
}