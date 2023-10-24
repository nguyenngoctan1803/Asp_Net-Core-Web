using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

namespace DoAn2VADT.Extension
{
    public static class decry
    {
        public static string MaMd5(string input)
{
    try
    {
        var encoder = new System.Text.UTF8Encoding();
        System.Text.Decoder utf8Decode = encoder.GetDecoder();
        byte[] todecodeByte = Convert.FromBase64String(input);
        int charCount = utf8Decode.GetCharCount(todecodeByte, 0, todecodeByte.Length);
        char[] decodedChar = new char[charCount];
        utf8Decode.GetChars(todecodeByte, 0, todecodeByte.Length, decodedChar, 0);
        string result = new String(decodedChar);
        return result;
    }
    catch (Exception ex)
    {
        throw new Exception("Error in base64Decode" + ex.Message);
    }
}
    }
}
