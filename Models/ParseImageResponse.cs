namespace HelperAPI.Models
{
    public class ParseImageResponse
    {
        public Info info { get; set; }
        public int rtnCode { get; set; }
        public string rtnMsg { get; set; }
        public bool isSuccess { get; set; }
    }
    public class Info
    {
        public string captcha { get; set; }
        public object validCode { get; set; }
        public string validTransactionId { get; set; }
    }
}
