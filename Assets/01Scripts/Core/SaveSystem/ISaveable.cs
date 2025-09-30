using Cysharp.Threading.Tasks;

public interface ISaveable
{
    public int SaveID { get; }
    public SaveManagerSO SaveManagerSO { get; }
    public UniTask<byte[]> ParsingToBytes();
    public UniTask ParsingFromBytes(byte[] bytes);
}