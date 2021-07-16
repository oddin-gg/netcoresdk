namespace Oddin.OddsFeedSdk.Sessions.Abstractions
{
    internal interface IOpenable
    {
        bool IsOpened();

        void Open();

        void Close();
    }
}
