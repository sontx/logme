namespace Client
{
    public interface IMessageDisplayer
    {
        void Append(string msg);
        void Clear();
    }
}
