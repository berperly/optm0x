namespace _0xOptimizer.Core.Logging
{
    public interface ILogSink
    {
        void Info(string msg);
        void Success(string msg);
        void Warn(string msg);
        void Error(string msg);
    }
}