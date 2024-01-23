namespace HongQuant.Analysis.Hong.Formula;
internal class FixList<T>
{
    private readonly List<T> m_list;
    private readonly int m_fixCount;
    public bool IsFull => m_list.Count == m_fixCount;

    public FixList(int count)
    {
        m_fixCount = count;
        m_list = new List<T>(m_fixCount);
    }

    /// <summary>
    /// 添加一个新元素，如果满了返回ture，否则返回false.
    /// 当添加元素满了，再添加一个元素会把第一个元素移除掉
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Add(T item)
    {
        if (m_fixCount == 0) return true;

        if (m_list.Count >= m_fixCount)
        {
            m_list.RemoveAt(0);
        }
        m_list.Add(item);
        return IsFull;
    }

    /// <summary>
    /// 计算除了最后一条数据的每一天数据总和
    /// </summary>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public List<double> SumWithoutLast(Func<T, double> keySelector)
    {
        List<double> sumList = [];
        for (int i = 0; i < m_list.Count - 1; i++)
        {
            sumList.Add(keySelector(m_list[i]));
        }
        return sumList;
    }
}
