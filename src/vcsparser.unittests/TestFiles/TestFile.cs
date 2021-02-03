using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.unittests.TestFiles
{
    public static class TestFile
    {
        public static string ChangesFiles1 => Read(nameof(ChangesFiles1));
        public static string DescribeFile1 => Read(nameof(DescribeFile1));
        public static string DescribeFile2 => Read(nameof(DescribeFile2));
        public static string DescribeFile3 => Read(nameof(DescribeFile3));
        public static string DescribeFile4 => Read(nameof(DescribeFile4));
        public static string GitExample1 => Read(nameof(GitExample1));
        public static string GitExample2 => Read(nameof(GitExample2));
        public static string GitExample3 => Read(nameof(GitExample3));
        public static string GitExample4 => Read(nameof(GitExample4));

        private static string Read(string resourceName)
        {
            var assembly = typeof(TestFile).Assembly;
            var fullResourceName = $"{typeof(TestFile).Namespace}.{resourceName}.txt";
            using var stream = assembly.GetManifestResourceStream(fullResourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
