namespace EnglishCardsBot.Domain.ValueObjects;

public static class ReviewIntervals
{
    // Интервалы между повторениями (10 шагов за ~3 месяца)
    // level N -> сколько дней добавить до следующего показа
    public static readonly int[] Days = { 1, 1, 2, 4, 7, 14, 21, 21, 19, 0 }; // суммарно ~90 дней
    
    public static int GetIntervalDays(int level)
    {
        if (level < 1 || level > Days.Length)
            return Days[0];
        return Days[level - 1];
    }
}

