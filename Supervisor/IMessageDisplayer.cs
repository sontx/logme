namespace LogMe.Supervisor
{
    public interface IMessageDisplayer
    {
        void Append(string msg);
        int Count { get; }
        void Clear();
    }
}
