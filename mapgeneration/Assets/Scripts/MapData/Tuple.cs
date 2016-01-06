public class Tuple<T,U,V>
{
	public T Item1 { get; private set; }
	public U Item2 { get; private set; }
	public V Item3 { get; private set; }
	
	public Tuple(T item1, U item2, V item3)
	{
		Item1 = item1;
		Item2 = item2;
		Item3 = item3;
	}
}

public static class Tuple
{
	public static Tuple<T, U, V> Create<T, U, V>(T item1, U item2, V item3)
	{
		return new Tuple<T, U, V>(item1, item2, item3);
	}
}