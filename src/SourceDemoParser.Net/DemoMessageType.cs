namespace SourceDemoParser
{
	public abstract class DemoMessageType
	{
		public int MessageType { get; }
		public string Name { get; }

		public DemoMessageType(int code)
		{
			MessageType = code;
			Name = GetType().Name;
		}

		public abstract IDemoMessage GetMessage();

		public override string ToString()
			=> Name;
	}
}