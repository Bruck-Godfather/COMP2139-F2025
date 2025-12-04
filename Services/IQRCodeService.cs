namespace COMP2138_ICE.Services
{
    public interface IQRCodeService
    {
        byte[] GenerateQRCode(string data);
    }
}
