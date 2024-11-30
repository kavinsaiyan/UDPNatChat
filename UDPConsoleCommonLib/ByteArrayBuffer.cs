public class ByteArrayBuffer
{
    private byte[] _buffer;
    private int _pos=0;

    public byte[] Buffer { get => _buffer; }
    public int Pos => _pos;

    public ByteArrayBuffer(int size = 4096)
    {
        _buffer = new byte[size];
        ResetPointer();
    }
    public void ResetPointer() => _pos = 0;

    public byte ReadByte()
    {
        return NetworkExtensions.ReadByte(ref _buffer, ref _pos);
    }
    public int ReadInt()
    {
        return NetworkExtensions.ReadInt(ref _buffer, ref _pos);
    }
    public string ReadString()
    {
        return NetworkExtensions.ReadString(ref _buffer, ref _pos);
    }
    public void  WriteByte(in byte byteData)
    {
        NetworkExtensions.WriteByte(ref _buffer, ref _pos, in byteData);
    }
    public void  WriteInt(in int number)
    {
        NetworkExtensions.WriteInt(ref _buffer, ref _pos, in number);
    }
    public void WriteString(in string line)
    {
        NetworkExtensions.WriteString(ref _buffer, ref _pos, in line);
    }

    public ArraySegment<byte> GetArraySlice()
    {
        return new ArraySegment<byte>(_buffer,0,_pos);
    }
}