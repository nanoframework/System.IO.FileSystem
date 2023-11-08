using System.Text;
using nanoFramework.TestFramework;

namespace System.IO.FileSystem.UnitTests
{
    [TestClass]
    public class FileUnitTests
    {
        protected const string Root = @"I:\";
        protected static readonly string SourceFile = $@"{Root}{nameof(FileUnitTests)}-Source.test";

        protected static readonly byte[] BinaryContent = Encoding.UTF8.GetBytes(TextContent);
        protected const string TextContent = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
    }
}
