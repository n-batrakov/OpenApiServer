using System;

using Microsoft.OpenApi.Writers;

using Newtonsoft.Json.Schema;

namespace ITExpert.OpenApi.Core.MockServer.Generation.Generators
{
    //TODO: Enchance generation quality
    //TODO: Pattern
    public class TextGenerator : IOpenApiExampleProvider
    {
        private Random Random { get; }

        public TextGenerator(Random random)
        {
            Random = random;
        }

        public bool TryWriteValue(IOpenApiWriter writer, JSchema schema)
        {
            var isText = schema.IsString() && schema.Format == null;
            if (!isText)
            {
                return false;
            }

            var minLength = (int)(schema.MinimumLength ?? 0);
            var maxLength = (int)(schema.MaximumLength ?? 10);
            var text = GetText(minLength, maxLength);
            writer.WriteValue(text);

            return true;
        }

        private string GetText(int minLength, int maxLength)
        {
            if (maxLength >= Text.Length)
            {
                return Text;
            }

            var startIndex = Random.Next(0, Text.Length - maxLength - 1);
            if (Text[startIndex] == ' ')
            {
                startIndex++;
            }

            var length = Random.Next(minLength, maxLength);

            return Text.Substring(startIndex, length);
        }

        private const string Text = 
                "Lorem ipsum dolor sit amet, iudico munere deseruisse his ad, ei tale rebum vidisse eum. " +
                "Nec dolor molestie an, vis munere dissentiunt in. Sea suscipit verterem ad, regione quae" +
                "stio omittantur vis ex. Natum verterem repudiare ad sit. Ut eum melius senserit convenir" +
                "e. Mea senserit erroribus philosophia te. His facilisi assueverit ex, simul dicant ut ne" +
                "c, no eros dolore reprimique quo. Vis ne soluta atomorum, ea mei odio partiendo. Mei ex " +
                "zril atomorum, tale perpetua no his. Timeam pertinacia vim no, te duo quidam expetenda s" +
                "imilique, usu an tota libris disputando. Cum nobis omittam corrumpit ut. Ne munere urban" +
                "itas per, at cum reque platonem. In case dicit essent eum. Id putant sensibus platonem v" +
                "is. Cu sea consequat interesset. Eu mei vidit vivendo propriae, regione liberavisse an h" +
                "is, has ad blandit postulant. At fugit scaevola sententiae usu, nam ut essent repudianda" +
                "e. Tollit aliquam vix an. Melius alienum ei sit. Ea omittam blandit voluptaria vis, mel " +
                "laudem dolorum inciderint at, mel tation vivendo et. Dicta tation qui et, ad mea nullam " +
                "eleifend. Est iriure quaeque no, in diam discere comprehensam mel, mollis verear alterum" +
                " ut usu. Cum id dicam altera, sea wisi commodo equidem ea, mei commodo mandamus assuever" +
                "it at. No accusam praesent est, usu congue gubergren et. Esse simul nec ex, cum id vitae" +
                " facilisi sadipscing, ea sea labitur pertinacia.";
    }
}