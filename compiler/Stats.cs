
namespace Ink {
    public struct Stats {

        public int words;

        public static Stats Generate(Ink.Parsed.Story story) {
            var stats = new Stats();

            var allText = story.FindAll<Ink.Parsed.Text>();

            // Count all the words across all strings
            stats.words = 0;
            foreach(var text in allText) {

                var wordsInThisStr = 0;
                var wasWhiteSpace = true;
                foreach(var c in text.text) {
                    if( c == ' ' || c == '\t' || c == '\n' || c == '\r' ) {
                        wasWhiteSpace = true;
                    } else if( wasWhiteSpace ) {
                        wordsInThisStr++;
                        wasWhiteSpace = false;
                    }
                }

                stats.words += wordsInThisStr;
            }

            return stats;
        }
    }
}