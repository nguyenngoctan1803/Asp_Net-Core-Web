namespace DoAn2VADT.Shared
{
    public static class Const
    {
        //Admin
        public const string ADMINSESSION = "ADMIN";
        public const string ADMINIDSESSION = "ADMINID";
        //User
        public const string USERSESSION = "USER";
        public const string USERIDSESSION = "USERID";
        //Import
        public const string IMPORTSESSION = "IMPORT";
        //Cart
        public const string CARTSESSION = "CART";
        //Order
        public const string ORDERSESSION = "ORDER";
        //Checkout
        public const string CHECKOUTSESSION = "CHECKOUT";
        public const string PAYWAY = "PAYWAY";
        public const string PAYSTATUS = "PAYSTATUS";
        //Sell
        public const string SELL = "SELL";
        public const string SELLID = "SELLID";
        // Reason
        public const string REFUSEREASON = "Lý do khác";
    }
    public static class StatusConst
    {
        // Order
        public const string WAITCONFIRM = "WAITCONFIRM";
        public const string CONFIRMED = "CONFIRMED";
        public const string EXPORT = "EXPORT";
        public const string EXPORTED = "EXPORTED";
        public const string SHIPPING = "SHIPPING";
        public const string RECEIVE = "RECEIVE";
        public const string PAID = "PAID";
        public const string DONE = "DONE";
        public const string CANCEL = "CANCEL";

        // Product
        public const string COMINGEND = "COMINGEND";
        public const string MORE = "MORE";
    }
    public static class PayConst
    {
        public const string ONLINE = "ONLINE";
        public const string OFFLINE = "OFFLINE";
    }
    public static class PayStatusConst
    {
        public const string DONE = "DONE";
        public const string NODONE = "NODONE";
    }
}
