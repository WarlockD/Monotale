using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace UndertaleResources
{
    #region Chunk and Stream interfaces
    // Been rewriting alot of code aniliziing these stupid chunks so thought I might as well make a class for it
    internal class ChunkEntry : IEquatable<ChunkEntry>
    {
        public int ChunkSize { get; private set; } // size is -1 if we don't have a size
        public int Position { get; private set; }
        public int Limit { get; private set; } // might be next offset if we have it
        public ChunkEntry(int position, int limit, int size) { Position = position; Limit = limit; ChunkSize = size; }
        public bool Equals(ChunkEntry obj)
        {
            return Position == obj.Position;
        }
        public override bool Equals(object obj)
        {
            ChunkEntry c = obj as ChunkEntry;
            if (c != null) return Equals(c);
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return Position; // this is the easiest as all Position are all diffrent
        }
    }
    internal class ChunkEntries : IEnumerable<ChunkEntry>//, IEnumerator<ChunkEntry>// list of offets that uses ChunkStream to move beetween
    {
        public static bool DebugOutput { get; set; }
        ChunkStream cs;
        int chunkLimit;
        int chunkStart;

        ChunkEntry[] entries;

        public int Count { get { return entries == null ? 0 : entries.Length; } }
        public ChunkEntry this[int i] { get { return entries[i]; } }
        public int Length { get { return entries.Length; } }
        void ReadEntries(bool rangeChecking)
        {
            int entriesCount = cs.ReadInt32();
            if (entriesCount > 100000) throw new ArgumentOutOfRangeException("Count", "Entries WAY out of range for resonable people (more than 100k)");
            int offset = 0, next_offset = 0, size = 0; // these always get assgiend but the compiler gets ansy if I don't put some number in it
            if (DebugOutput) System.Diagnostics.Debug.WriteLine("ChunkEntries: {0}", entriesCount);
            if (entriesCount == 0)
            {
                // empty entries, so set it up as one
                this.entries = null;
                return;
            }
            else if (entriesCount == 1)
            {
                offset = cs.ReadInt32();
                if (rangeChecking && (offset >= chunkLimit || offset <= chunkStart)) throw new ArgumentOutOfRangeException("Index 0", "Out of Chunk range");
                this.entries = new ChunkEntry[] { new ChunkEntry(offset, chunkLimit, chunkLimit - offset) };
                return;
            } // Solve all this above for the two special cases.  This code dosn't look elegant at all, but it works
            List<ChunkEntry> entries = new List<ChunkEntry>(entriesCount);
            int[] offsets = cs.ReadInt32(entriesCount);
            for (int i = 0; i < (entriesCount - 1); i++)
            {
                offset = offsets[i];
                if (rangeChecking && (offset >= chunkLimit || offset <= chunkStart)) throw new ArgumentOutOfRangeException("Index " + i, "Out of Chunk range");
                next_offset = offsets[i + 1];
                size = next_offset - offset;
                entries.Add(new ChunkEntry(offset, next_offset, size));
                offset = next_offset;
                offset = offsets[i];
                // entries[i] = new ChunkEntry(offset, next_offset, size);
            }

            if (rangeChecking && (next_offset >= chunkLimit || next_offset <= chunkStart)) throw new ArgumentOutOfRangeException("Index " + (entriesCount - 1), "Out of Chunk Limit");
            double average = entries.Average(c => c.ChunkSize);
            double sumOfSquaresOfDifferences = entries.Select(c => (c.ChunkSize - average) * (c.ChunkSize - average)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / entries.Count);

            if (((int)sd == 0) && size == (int)Math.Round(average))
            {// they are all fixed size, so its easy
                entries.Add(new ChunkEntry(next_offset, next_offset + size, size)); // ChunkLimit should eqal = next_offset + size, but it dosn't have to at this point
                if (DebugOutput) System.Diagnostics.Debug.WriteLine("This has a Fixed size of {0} after testing {1} entries", size, entriesCount);
            }
            else
            {
                double dsize = Math.Abs((chunkLimit - next_offset) - average);
                if (dsize <= sd) entries.Add(new ChunkEntry(next_offset, next_offset, chunkLimit - next_offset)); // then the end of the chunkLimit IS the last offset
                else
                {
                    if (DebugOutput)
                    {
                        System.Diagnostics.Debug.WriteLine("Could not find end of next offset, so wild guess from {0} entries", entriesCount);
                        System.Diagnostics.Debug.WriteLine("ChunkEntrie: Average: {0:0.00}  SumOfSquares: {1:0.00}  StandardDeveation {2:0.00}  dsize {3:0.00}", average, sumOfSquaresOfDifferences, sd, dsize);

                    }
                    entries.Add(new ChunkEntry(next_offset, chunkLimit, -1)); // then the end of the chunkLimit IS the last offset
                }
            }
            this.entries = entries.ToArray();
        }

        public ChunkEntries(ChunkStream cs, int chunkLimit, bool rangeChecking = true)
        {
            this.cs = cs;
            this.chunkStart = cs.Position;
            this.chunkLimit = chunkLimit;
            ReadEntries(rangeChecking);
        }
        public ChunkEntries(ChunkStream cs, int chunkStart, int chunkLimit, bool rangeChecking = true)
        {
            this.cs = cs;
            this.chunkStart = chunkStart;
            this.chunkLimit = chunkLimit;
            cs.PushSeek(chunkStart);
            ReadEntries(rangeChecking);
            cs.PopPosition();
        }
        class ChunkEntriesEnumerator : IEnumerator<ChunkEntry>
        {
            int pos;
            ChunkEntry[] entries;
            ChunkStream cs;
            ChunkEntry current;
            public ChunkEntriesEnumerator(ChunkStream cs, ChunkEntry[] entries)
            {
                this.cs = cs;
                this.entries = entries;
                this.pos = 0;
                this.current = null;
                cs.PushPosition();
            }
            public ChunkEntry Current { get { return current; } }
            object IEnumerator.Current { get { return current; } }
            public void Dispose() { cs.PopPosition(); }
            public bool MoveNext()
            {
                if (entries != null && pos < entries.Length)
                {
                    // if (DebugOutput) System.Diagnostics.Debug.WriteLine("Moving to {0} out of {1}", pos, entries.Length);
                    ChunkEntry e = entries[pos++];
                    current = e;
                    cs.Position = e.Position;
                    return true;
                }
                current = null;
                return false;
            }
            public void Reset() { pos = 0; }
        }
        // Must implement GetEnumerator, which returns a new StreamReaderEnumerator.
        public IEnumerator<ChunkEntry> GetEnumerator() { return new ChunkEntriesEnumerator(cs, entries); }
        IEnumerator IEnumerable.GetEnumerator() { return (IEnumerator)this; }
    }
    // http://stackoverflow.com/questions/31078598/c-sharp-create-a-filestream-with-an-offset
    // This class saved me for a workaround for reading a bitmap out of an exisiting file
    internal class ChunkStreamOffset : Stream
    {
        private readonly Stream instance;
        private readonly long offset;

        public static Stream Decorate(Stream instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");

            Stream decorator = new ChunkStreamOffset(instance);
            return decorator;
        }

        private ChunkStreamOffset(Stream instance)
        {
            this.instance = instance;
            this.offset = instance.Position;
        }

        #region override methods and properties pertaining to the file position/length to transform the file positon using the instance's offset

        public override long Length
        {
            get { return instance.Length - offset; }
        }

        public override void SetLength(long value)
        {
            instance.SetLength(value + offset);
        }

        public override long Position
        {
            get { return instance.Position - this.offset; }
            set { instance.Position = value + this.offset; }
        }

        public override bool CanRead
        {
            get
            {
                return instance.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return instance.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return instance.CanWrite;
            }
        }

        // etc.

        #endregion

        #region override all other methods and properties as simple pass-through calls to the decorated instance.

        public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            return instance.BeginRead(array, offset, numBytes, userCallback, stateObject);
        }

        public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            return instance.BeginWrite(array, offset, numBytes, userCallback, stateObject);
        }

        public override void Flush()
        {
            instance.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // if (origin == SeekOrigin.Begin) offset += this.offset;
            return instance.Seek(offset, origin);// - this.offset;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return instance.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            instance.Write(buffer, offset, count);
        }
        #endregion
    }
    /// <summary>
    /// This class takes a stream and limits it at a starting offset from the Begining and optionaly, and ending Length
    /// From the users standpoint, this stream works just like a normal stream. This will throw an IO error if you go outside
    /// of the limits of the stream
    /// </summary>
    internal class OffsetStream : Stream
    {
        public class OffsetStreamLimitException : ArgumentException
        {
            public OffsetStreamLimitException() : base("Cannot go outside the limits of an OffsetStream") { }
        }
        Stream BaseStream;
        long _length;
        long _start;
        public OffsetStream(Stream s, long start, long length)
        {
            BaseStream = s;
            _start = start;
            _length = length;
            Position = 0;
        }
        public OffsetStream(Stream s, long start) : this(s, start, s.Length) { }
        public override bool CanRead
        {
            get
            {
                return BaseStream.CanRead;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return BaseStream.CanSeek;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return BaseStream.CanWrite;
            }
        }
        public override long Length { get { return _length; } }
        public override long Position
        {
            get
            {
                return BaseStream.Position - _start;
            }

            set
            {
                long newPos = value + _start;
                if (newPos < 0 || newPos > (_length + _start)) throw new OffsetStreamLimitException();
                BaseStream.Position = newPos;
            }
        }

        public override void Flush() { BaseStream.Flush(); }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int ret = BaseStream.Read(buffer, offset, count);
            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    offset = offset + _start;
                    break;
                case SeekOrigin.End:
                    offset = offset + _start + _length;
                    break;
            }
            //    if (newPos < 0 || newPos > _length) throw new OffsetStreamLimitException();
            BaseStream.Seek(offset, origin);
            return Position;
        }

        public override void SetLength(long value) { throw new NotImplementedException(); }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long limit = Position + count;
            if (limit > _length) throw new OffsetStreamLimitException();
            BaseStream.Write(buffer, offset, count);
        }
    }

    class ChunkStream : BinaryReader
    {
        Stack<int> posStack = new Stack<int>();
        // used on reading strings so we don't go creating a new one for thousands of things
        Dictionary<int, string> stringCache = new Dictionary<int, string>();

        public byte[] ChunkData { get; private set; }
        public ChunkStream(Stream s) : base(s) { DebugPosition = false; ChunkData = null; }
        public ChunkStream(Stream s, Encoding e) : base(s, e) { DebugPosition = false; ChunkData = null; }
        public ChunkStream(Stream s, Encoding e, bool leaveOpen) : base(s, e, leaveOpen) { DebugPosition = false; ChunkData = null; }
        public ChunkStream(byte[] chunk) : base(new MemoryStream(chunk, false)) { DebugPosition = false; ChunkData = chunk; }
        public bool DebugPosition { get; set; }
        public int Position { get { return (int)BaseStream.Position; } set { BaseStream.Position = value; } }
        public int Length { get { return (int)BaseStream.Length; } }

        public Stream StreamFromPosition()
        {
            return ChunkStreamOffset.Decorate(BaseStream);
        }
        public Stream StreamFromPosition(int position)
        {
            PushSeek(position);
            Stream s = ChunkStreamOffset.Decorate(BaseStream);
            PopPosition();
            return s;
        }
        public void PushPosition()
        {
            BaseStream.Flush();
            posStack.Push(Position);
        }
        public void PushSeek(int position)
        {
            PushPosition();
            Position = position;
        }
        public void PopPosition()
        {
            BaseStream.Flush();
            Position = posStack.Pop();
        }
        public ChunkStream readChunk(int chunkStart, int chunkEnd)
        {
            PushSeek(chunkStart);
            byte[] data = ReadBytes((int)(chunkEnd - chunkStart));
            PopPosition();
            return new ChunkStream(data);
        }
        public uint[] ReadUInt32(int count, int position)
        {
            PushSeek(position);
            uint[] ret = ReadUInt32(count);
            PopPosition();
            return ret;
        }
        public uint[] ReadUInt32(int count)
        {
            byte[] bytes = ReadBytes(count * sizeof(uint));
            uint[] intArray = new uint[count];
            Buffer.BlockCopy(bytes, 0, intArray, 0, intArray.Length * sizeof(uint));
            return intArray;
        }
        public int[] ReadInt32(int count, int position)
        {
            PushSeek(position);
            int[] ret = ReadInt32(count);
            PopPosition();
            return ret;
        }
        public int[] ReadInt32(int count)
        {
            // after looking at the code for ReadInt32 at microsoft, its better to read a buffer
            // of bytes and convert that then using ReadInt repeadedly.  Looks like alot of overhead with it
            byte[] bytes = ReadBytes(count * sizeof(int));
            int[] intArray = new int[count];
            Buffer.BlockCopy(bytes, 0, intArray, 0, intArray.Length * sizeof(int));
            return intArray;
        }
        public short[] ReadInt16(int count, int position)
        {
            PushSeek(position);
            short[] ret = ReadInt16(count);
            PopPosition();
            return ret;
        }
        public short[] ReadInt16(int count)
        {
            byte[] bytes = ReadBytes(count * sizeof(short));
            short[] shortArray = new short[count];
            Buffer.BlockCopy(bytes, 0, shortArray, 0, shortArray.Length * sizeof(short));
            return shortArray;
        }
        public int[] getOffsetEntries()
        {
            int count = ReadInt32();
            return count > 0 ? ReadInt32(count) : null;
        }
        public int[] getOffsetEntries(int start)
        {
            PushSeek(start);
            var entries = getOffsetEntries();
            PopPosition();
            return entries;
        }
        /*
        public int[] CollectEntries(int limit, bool advance = true, bool ignorelimit = false)
        {
            List<int> entries = new List<int>();
            if (!advance) PushPosition();
            int entriesCount = ReadInt32();
            int entryStart = (int)(BaseStream.Position + entriesCount);
            for (int i = 0; i < entriesCount; i++)
            {
                int entry = ReadInt32();
                if (!ignorelimit && (entry < entryStart || entry > limit)) throw new ArgumentOutOfRangeException("Index_" + i, "Offset out of limit");
                entries.Add(entry);
            }
            if(!advance) PopPosition();
            return entries.ToArray();
        }
        public int[] CollectEntries(bool advance = true)
        {
            return CollectEntries(0, advance, true);
        }
        */
        public bool readIntBool()
        {
            int b = this.ReadInt32();
            if (b != 1 && b != 0) throw new Exception("Expected bool to be 0 or 1");
            return b != 0;
        }
        public string readFixedString(int count)
        {
            byte[] bytes = ReadBytes(count);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        public string readStringFromOffset()
        {
            int offset = this.ReadInt32();
            PushSeek(offset);
            string s = ReadVString();
            PopPosition();
            return s;
        }
        string ReadVString() // We shouldn't throw here
        {
            string str;
            int posStart = Position;
            if (stringCache.TryGetValue(posStart, out str)) return str;
            List<byte> bytes = new List<byte>();
            for (;;)
            {
                if (Position >= Length) throw new EndOfStreamException("End of stream before end of string");
                byte b = ReadByte();
                if (b == 0) break;
                bytes.Add(b);
            }
            if (bytes.Count == 0) return null; // null if we just read a 0
            str = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
            stringCache[posStart] = str;
            return str;
        }
        string ReadVString(int position)
        {
            string s;
            int posStart = Position; // we do it again here for speed
            if (stringCache.TryGetValue(Position, out s)) return s;
            s = ReadVString();
            Position = posStart;
            return s;
        }

    }
    #endregion
    interface NamedResrouce
    {
        string Name { get; }
    }
    public abstract class FilePosition : IComparable<FilePosition>, IEquatable<FilePosition>
    { //create comparer
        public int Position { get; protected set; }
        public int Index { get; protected set; }
        public FilePosition()
        {
            Index = -1;
            Position = -1;
        }
        internal abstract void Read(ChunkStream r, int index);

        public int CompareTo(FilePosition other)
        {
            return Position.CompareTo(other.Position);
        }
        public override int GetHashCode()
        {
            return Position;
        }
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;
            if (object.ReferenceEquals(obj, this)) return true;
            FilePosition d = obj as FilePosition;
            if (d != null) return Equals(d);
            return d != null && Equals(d);
        }
        public bool Equals(FilePosition other)
        {
            return other.Position == Position;
        }
        public override string ToString()
        {
            NamedResrouce ns = this as NamedResrouce;
            return ns != null ? ns.Name : this.GetType().Name;
        }
    }
    public class Texture : FilePosition
    {
        int _pngLength;
        int _pngOffset;
        // I could read a bitmap here like I did in my other library however
        // monogame dosn't use Bitmaps, neither does unity, so best just to make a sub stream
        public Stream GetTextureStream(Stream s)
        {
            s.Position = _pngOffset;
            return new OffsetStream(s, _pngOffset, _pngLength);
        }
        static readonly byte[] pngSigBytes = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
        static readonly string pngSig = System.Text.Encoding.UTF8.GetString(pngSigBytes);
        internal override void Read(ChunkStream r, int index)
        {
            Position = r.Position;
            Index = index;
            int dummy = r.ReadInt32(); // Always 1
            _pngOffset = r.ReadInt32(); // offset to texture
            r.Position = _pngOffset;
            string sig = r.readFixedString(8);

            if (sig != pngSig) throw new Exception("Texture not a png");
            // to get the texture lengh, we have to read all the chunks and add them together
            // once this is done, we can create a proper stream
            // We are doing it this way so we can read the entire stream just once for whatever
            // api needs it

            _pngLength = 0;
            int length;
            string chunk;
            byte[] edianInt = new byte[4];
            do
            {
                r.Read(edianInt, 0, 4);
                length = (edianInt[0] << 24) | (edianInt[1] << 16) | (edianInt[2] << 8) | edianInt[3];
                chunk = r.readFixedString(4);
                if (length < 0) throw new Exception("Ugh, have to fix this");
                r.Position += length + 4; // plus the CRC
            } while (chunk != "IEND");
            _pngLength = r.Position - Position;

            //  OffsetStream
        }
    }
    public class AudioFile : FilePosition, NamedResrouce
    {
        public string Name { get; private set; }
        public int AudioType { get; private set; }
        public string Extension { get; private set; }
        public string FileName { get; private set; }
        public int Effects { get; private set; }
        public float Volume { get; private set; }
        public float Pan { get; private set; }
        public int Other { get; private set; }
        public int SoundIndex { get; private set; }
        internal override void Read(ChunkStream r, int index)
        {
            Position = r.Position;
            Index = index;
            Name = string.Intern(r.readStringFromOffset()); // be sure to intern the name
            AudioType = r.ReadInt32();
            // Ok, 101 seems to be wave files in the win files eveything else is mabye exxternal?
            // 100 is mp3 ogg?
            // found no other types in there.  
            // Debug.Assert(audio.audioType == 101 || audio.audioType == 100);
            Extension = r.readStringFromOffset();
            FileName = r.readStringFromOffset();
            Effects = r.ReadInt32();
            Volume = r.ReadSingle();
            Pan = r.ReadSingle();
            Other = r.ReadInt32();
            SoundIndex = r.ReadInt32();
        }
    }
    public class SpriteFrame : FilePosition
    {
        public short X { get; private set; }
        public short Y { get; private set; }
        public short Width { get; private set; }
        public short Height { get; private set; }
        public short OffsetX { get; private set; }
        public short OffsetY { get; private set; }
        public short CropWidth { get; private set; }
        public short CropHeight { get; private set; }
        public short OriginalWidth { get; private set; }
        public short OriginalHeight { get; private set; }
        public short TextureIndex { get; private set; }
        internal override void Read(ChunkStream cs, int index)
        {
            Position = cs.Position;
            Index = index;
            X = cs.ReadInt16();
            Y = cs.ReadInt16();
            Width = cs.ReadInt16();
            Height = cs.ReadInt16();
            OffsetX = cs.ReadInt16();
            OffsetY = cs.ReadInt16();
            CropWidth = cs.ReadInt16();
            CropHeight = cs.ReadInt16();
            OriginalWidth = cs.ReadInt16();
            OriginalHeight = cs.ReadInt16();
            TextureIndex = cs.ReadInt16();
        }
    }
    public class Sprite : FilePosition, NamedResrouce
    {
        public string Name { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Flags { get; private set; }
        public int Width0 { get; private set; }
        public int Height0 { get; private set; }
        public int Another { get; private set; }
        public int[] Extra { get; private set; }
        public SpriteFrame[] Frames { get; private set; }
        internal override void Read(ChunkStream r, int index)
        {
            Position = r.Position;
            Index = index;
            Name = string.Intern(r.readStringFromOffset()); // be sure to intern the name
            Width = r.ReadInt32();
            Height = r.ReadInt32();
            Flags = r.ReadInt32();
            Width0 = r.ReadInt32();
            Height0 = r.ReadInt32();
            Another = r.ReadInt32();
            Extra = r.ReadInt32(7);
            Frames = UndertaleResrouce.ArrayFromOffset<SpriteFrame>(r);
            // bitmask is here
            int haveMask = r.ReadInt32();
            if (haveMask != 0)
            { // have mask?
                int stride = (Width % 8) != 0 ? Width + 1 : Width;
                //	std::vector<uint8_t>* mask = new std::vector<uint8_t>();
                //	mask->resize(stride * header.height);
                //	r.read(mask->data(), mask->size());
                //	_spriteMaskLookup.emplace(std::make_pair(name, mask));
            }
        }

    }
    public class UObject : FilePosition, NamedResrouce
    {
        public string Name { get; private set; }
        public Sprite Sprite { get; private set; }

        public bool Visible;
        public bool Solid;
        public int Depth;
        public bool Persistent;
        public int ParentIndex;
        public int Mask;

        internal override void Read(ChunkStream r, int index)
        {
            Position = r.Position;
            Index = index;
            Name = string.Intern(r.readStringFromOffset());
            int spriteIndex = r.ReadInt32();
            if (spriteIndex < 0)
                Sprite = null;
            else
                Sprite = UndertaleResrouce.SpriteAtIndex(spriteIndex);
            Visible = r.readIntBool();
            Solid = r.readIntBool();
            Depth = r.ReadInt32();
            Persistent = r.readIntBool();
            ParentIndex = r.ReadInt32();
            Mask = r.ReadInt32();
        }
    };
    public class Background : FilePosition, NamedResrouce
    {
        public string Name { get; private set; }
        public bool Trasparent { get; private set; }
        public bool Smooth { get; private set; }
        public bool Preload { get; private set; }
        public SpriteFrame Frame { get; private set; }
        internal override void Read(ChunkStream r, int index)
        {
            Position = r.Position;
            Index = index;
            Name = string.Intern(r.readStringFromOffset());
            Trasparent = r.readIntBool();
            Smooth = r.readIntBool();
            Preload = r.readIntBool();
            int offset = r.ReadInt32();
            r.Position = offset;
            Frame = new SpriteFrame();
            Frame.Read(r, -1);
        }
    };
    public class Room : FilePosition, NamedResrouce
    {
        public class View : FilePosition
        {
            public bool Visible;
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public int Port_X;
            public int Port_Y;
            public int Oort_Width;
            public int Port_Height;
            public int Border_X;
            public int Border_Y;
            public int Speed_X;
            public int Speed_Y;
            public int ViewIndex;
            internal override void Read(ChunkStream r, int index)
            {
                Position = r.Position;
                Index = index;
                Visible = r.readIntBool();
                X = r.ReadInt32();
                Y = r.ReadInt32();
                Width = r.ReadInt32();
                Height = r.ReadInt32();
                Port_X = r.ReadInt32();
                Port_Y = r.ReadInt32();
                Oort_Width = r.ReadInt32();
                Port_Height = r.ReadInt32();
                Border_X = r.ReadInt32();
                Border_Y = r.ReadInt32();
                Speed_X = r.ReadInt32();
                Speed_Y = r.ReadInt32();
                Index = r.ReadInt32();
            }
        }
        public class Background : FilePosition
        {
            public bool Visible;
            public bool Foreground;
            public int BackgroundIndex;
            public int X;
            public int Y;
            public int TiledX;
            public int TiledY;
            public int SpeedX;
            public int SpeedY;
            public bool Stretch;
            internal override void Read(ChunkStream r, int index)
            {
                Position = r.Position;
                Index = index;
                Visible = r.readIntBool();
                Foreground = r.readIntBool();
                BackgroundIndex = r.ReadInt32();
                X = r.ReadInt32();
                Y = r.ReadInt32();
                TiledX = r.ReadInt32();
                TiledY = r.ReadInt32();
                SpeedX = r.ReadInt32();
                SpeedY = r.ReadInt32();
                Stretch = r.readIntBool();
            }
        };
        public class Instance : FilePosition
        {
            public int X;
            public int Y;
            public int ObjectIndex;
            public int Id;
            public int CodeOffset;
            public float Scale_X;
            public float Scale_Y;
            public int Colour;
            public float Rotation;
            internal override void Read(ChunkStream r, int index)
            {
                Position = r.Position;
                Index = index;
                X = r.ReadInt32();
                Y = r.ReadInt32();
                ObjectIndex = r.ReadInt32();
                Id = r.ReadInt32();
                CodeOffset = r.ReadInt32();
                Scale_X = r.ReadSingle();
                Scale_Y = r.ReadSingle();
                Colour = r.ReadInt32();
                Rotation = r.ReadSingle();
            }
        };
        public class Tile : FilePosition
        {
            public int X;
            public int Y;
            public int BackgroundIndex;
            public int OffsetX;
            public int OffsetY;
            public int Width;
            public int Height;
            public int Depth;
            public int Id;
            public float ScaleX;
            public float ScaleY;
            public int Blend;
            public int Ocupancy;
            internal override void Read(ChunkStream r, int index)
            {
                Position = r.Position;
                Index = index;
                X = r.ReadInt32();
                Y = r.ReadInt32();
                BackgroundIndex = r.ReadInt32();
                OffsetX = r.ReadInt32();
                OffsetY = r.ReadInt32();
                Width = r.ReadInt32();
                Height = r.ReadInt32();
                Depth = r.ReadInt32();
                Id = r.ReadInt32();
                ScaleX = r.ReadSingle();
                ScaleY = r.ReadSingle();
                int mixed = r.ReadInt32();
                Blend = mixed & 0x00FFFFFF;
                Ocupancy = mixed >> 24;
            }
        };
        public string Name { get; private set; }
        public string Caption { get; private set; }

        public int Width;
        public int Height;
        public int Speed;
        public int Persistent;
        public int Colour;
        public int Show_colour;
        public int CodeOffset;
        public int Flags;
        public Background[] Backgrounds;
        public View[] Views;
        public Instance[] Objects;
        public Tile[] Tiles;
        internal override void Read(ChunkStream r, int index)
        {
            Position = r.Position;
            Index = index;
            Name = r.readStringFromOffset();
            Caption = r.readStringFromOffset();
            Width = r.ReadInt32();
            Height = r.ReadInt32();
            Speed = r.ReadInt32();
            Persistent = r.ReadInt32();
            Colour = r.ReadInt32();
            Show_colour = r.ReadInt32();
            CodeOffset = r.ReadInt32();
            Flags = r.ReadInt32();
            int backgroundsOffset = r.ReadInt32();
            int viewsOffset = r.ReadInt32();
            int instancesOffset = r.ReadInt32();
            int tilesOffset = r.ReadInt32();
            Backgrounds = UndertaleResrouce.ArrayFromOffset<Background>(r, backgroundsOffset);
            Views = UndertaleResrouce.ArrayFromOffset<View>(r, viewsOffset);
            Objects = UndertaleResrouce.ArrayFromOffset<Instance>(r, instancesOffset);
            Tiles = UndertaleResrouce.ArrayFromOffset<Tile>(r, tilesOffset);
        }
    }
    public class Font : FilePosition, NamedResrouce
    {
        public class Glyph : FilePosition
        {
            public short ch;
            public short x;
            public short y;
            public short width;
            public short height;
            public short shift;
            public short offset;
            internal override void Read(ChunkStream r, int index)
            {
                Position = r.Position;
                Index = index;
                ch = r.ReadInt16();
                x = r.ReadInt16();
                y = r.ReadInt16();
                width = r.ReadInt16();
                height = r.ReadInt16();
                shift = r.ReadInt16();
                offset = r.ReadInt16();
            }
        }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Size { get; private set; }
        public bool Bold { get; private set; }
        public bool Italic { get; private set; }
        public char FirstChar { get; private set; }
        public char LastChar { get; private set; }
        public int AntiAlias { get; private set; }
        public int CharSet { get; private set; }

        public SpriteFrame Frame { get; private set; }
        public float ScaleW { get; private set; }
        public float ScaleH { get; private set; }
        public Glyph[] Glyphs { get; private set; }
        internal override void Read(ChunkStream r, int index)
        {
            Position = r.Position;
            Index = index;
            Name = r.readStringFromOffset();
            Description = r.readStringFromOffset();
            Size = r.ReadInt32();
            Bold = r.readIntBool();
            Italic = r.readIntBool();;
            int flag = r.ReadInt32();
            FirstChar = (char)(flag & 0xFFFF);
            CharSet = (flag >> 16) & 0xFF;
            AntiAlias = (flag >> 24) & 0xFF;
            LastChar = (char)r.ReadInt32();
            Frame = new SpriteFrame();
            r.PushSeek(r.ReadInt32());
            Frame.Read(r, -1);
            r.PopPosition();
            ScaleW = r.ReadSingle();
            ScaleH = r.ReadSingle();
            Glyphs = UndertaleResrouce.ArrayFromOffset<Glyph>(r);
        }

      
    }
    public class RawAudio : FilePosition
    {
        public int Size { get; private set;  }
        public byte[] RawSound { get; private set; }
        internal override void Read(ChunkStream r, int index)
        {
            Position = r.Position;
            Index = index;
            Size = r.ReadInt32();
            RawSound = r.ReadBytes(Size);
        }
    }
    public class UndertaleResrouce
    {
        static bool loaded = false;
        static List<string> stringList;
        static List<Texture> textures = new List<Texture>();
        static List<Sprite> sprites = new List<Sprite>();
        static List<UObject> objects = new List<UObject>();
        static List<Room> rooms = new List<Room>();
        static List<Background> backgrounds = new List<Background>();
        static List<AudioFile> sounds = new List<AudioFile>();
        static List<RawAudio> rawAudio = new List<RawAudio>();
        static List<Font> fonts = new List<Font>();

        
        static Dictionary<string, FilePosition> namedResourceLookup = new Dictionary<string, FilePosition>();

        public static IReadOnlyList<Texture> Textures { get { return textures; } }
        public static IReadOnlyList<Sprite> Sprites { get { return sprites; } }
        public static IReadOnlyList<UObject> Objects { get { return objects; } }
        public static IReadOnlyList<Room> Rooms { get { return rooms; } }
        public static IReadOnlyList<Background> Backgrounds { get { return backgrounds; } }
        public static IReadOnlyList<AudioFile> Sounds { get { return sounds; } }

        static public AudioFile AudioAtIndex(int index)
        {
            if (index < 0 || index > sounds.Count) throw new IndexOutOfRangeException("Sprite index out of range");
            return sounds[index];
        }
        static public byte[] AudioDataAtIndex(int index)
        {
            var file = AudioAtIndex(index);
            if (index < 0 || index > sounds.Count) throw new IndexOutOfRangeException("Sprite index out of range");
            return file.SoundIndex >= 0 ? rawAudio[file.SoundIndex].RawSound : null;
        }
        static public Background BackgroundAtIndex(int index)
        {
            if (index < 0 || index > backgrounds.Count) throw new IndexOutOfRangeException("Sprite index out of range");
            return backgrounds[index];
        }
        static public Sprite SpriteAtIndex(int index)
        {
            if (index < 0 || index > sprites.Count) throw new IndexOutOfRangeException("Sprite index out of range");
            return sprites[index];
        }
        static public UObject ObjectAtIndex(int index)
        {
            if (index < 0 || index > objects.Count) throw new IndexOutOfRangeException("Sprite index out of range");
            return objects[index];
        }
        static public Room RoomAtIndex(int index)
        {
            if (index < 0 || index > rooms.Count) throw new IndexOutOfRangeException("Sprite index out of range");
            return rooms[index];
        }
        /// <summary>
        /// This returns a substream to data_win so it can be loaded using whatever api you use to load textures
        /// with
        /// </summary>
        /// <param name="data_win">The Stream to the data.win file</param>
        /// <param name="index">Texture index</param>
        /// <returns></returns>
        static public Stream TextureStreamAtIndex(Stream data_win, int index)
        {
            if (index < 0 || index > textures.Count) throw new IndexOutOfRangeException("textures index out of range");
            return textures[index].GetTextureStream(data_win);
        }
        static public IEnumerable<Stream> AllTextures(Stream data_win)
        {
            foreach (var s in textures) yield return s.GetTextureStream(data_win);
        }
        static internal T[] ArrayFromOffset<T>(ChunkStream r, int offset) where T : FilePosition, new()
        {
            r.PushSeek(offset);
            T[] ret = ArrayFromOffset<T>(r);
            r.PopPosition();
            return ret;
        }
        static internal T[] ArrayFromOffset<T>(ChunkStream r) where T : FilePosition, new()
        {
            int count = r.ReadInt32();
            if (count < 0) return new T[] { };
            var offsets = r.ReadInt32(count);
            r.PushPosition(); // We want to return at the position at the end of the offset table
            T[] data = new T[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                r.Position = offsets[i];
                T obj = new T();
                obj.Read(r, i);
                data[i] = obj;
            }
            r.PopPosition();
            return data;
        }
        static public bool TryGetResource<T>(string name, out T ret) where T : FilePosition
        {
            FilePosition test;
            if (namedResourceLookup.TryGetValue(name, out test) && test is T)
            {
                ret = test as T;
                return true;
            }
            ret = default(T);
            return false;
        }

        static void doStrings(ChunkStream r, int chunkStart, int chunkLimit)
        {
            stringList = new List<string>();
            ChunkEntries entries = new ChunkEntries(r, chunkStart, chunkLimit);
            foreach (ChunkEntry e in entries)
            {
                int string_size = r.ReadInt32(); //size 
                byte[] bstr = r.ReadBytes(string_size);
                string s = System.Text.Encoding.UTF8.GetString(bstr, 0, string_size);
                stringList.Add(s);
            }
        }
        class Chunk
        {
            public readonly int start;
            public readonly int end;
            public readonly int size;
            public readonly string name;
            public Chunk(string name, int start, int size) { this.name = name; this.start = start; this.end = start + size; this.size = size; }
        }
        static void ReadList<T>(List<T> list, ChunkStream r, int start, int end) where T : FilePosition, new()
        {
            bool isNamedResouce = typeof(T).GetInterfaces().Contains(typeof(NamedResrouce));
            list.Clear();
            ChunkEntries entries = new ChunkEntries(r, start, end);
            foreach (ChunkEntry e in entries)
            {
                T t = new T();
                t.Read(r, list.Count);
                list.Add(t);
                if (isNamedResouce)
                {
                    NamedResrouce nr = t as NamedResrouce;
                    namedResourceLookup.Add(nr.Name, t);
                }
            }
        }
        public static void DebugPring()
        {
            using (StreamWriter sr = new StreamWriter("object_info.txt"))
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    var o = objects[i];
                    sr.Write("{0,-4}: Name: {1:-20}", i, o.Name);
                    if (o.ParentIndex > 0) sr.Write("  Parent({0}): {1}", o.ParentIndex, objects[o.ParentIndex].Name);
                    sr.WriteLine();
                }
            }
            using (StreamWriter sr = new StreamWriter("sprite_info.txt"))
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    var o = sprites[i];
                    sr.Write("{0,-4}: Name: {1:-20}", i, o.Name);
                    sr.Write("  Frames: {0}", o.Frames.Length);
                    sr.WriteLine();
                }
            }
            using (StreamWriter sr = new StreamWriter("font_info.txt"))
            {
                for (int i = 0; i < fonts.Count; i++)
                {
                    var o = fonts[i]; 
                    sr.Write("{0,-4}: Name: {1,-20} Description: {2,-20} Size: {3,-4}", i, o.Name, o.Description, o.Size);
                    sr.Write(" Bold:{0}  Italix:{1} Scale({2},{3})", o.Bold, o.Italic, o.ScaleW, o.ScaleH);
                    sr.WriteLine();
                }
            }
            using (StreamWriter sr = new StreamWriter("room_info.txt"))
            {
                for (int i = 0; i < rooms.Count; i++)
                {
                    var o = rooms[i];
                    sr.Write("{0,-4}: Name: {1:-20} Size({2},{3})", i, o.Name, o.Width, o.Height);
                    sr.WriteLine();
                    if (o.Objects.Length > 0)
                        for (int j = 0; j < o.Objects.Length; j++)
                        {
                            var oo = o.Objects[j];
                            var obj = objects[oo.Index];
                            sr.WriteLine("       Object: {0}  Pos({1},{2}", obj.Name, oo.X, oo.Y);
                        }
                }
            }
        }
        public static void LoadResrouces(Stream s)
        {
            if (loaded) return;
            ChunkStream r = new ChunkStream(s);
            int full_size = r.Length;
            Chunk chunk = null;
            Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();
            while (r.BaseStream.Position < full_size)
            {
                string chunkName = r.readFixedString(4);
                int chunkSize = r.ReadInt32();
                int chuckStart = r.Position;
                chunk = new Chunk(chunkName, chuckStart, chunkSize);
                chunks[chunkName] = chunk;
                if (chunkName == "FORM") full_size = chunkSize; // special case for form
                else r.Position = chuckStart + chunkSize; // make sure we are always starting at the next chunk               
            }
            // Don't REALLY need strings here
            //if (chunks.TryGetValue("STRG", out chunk)) doStrings(r, chunk.start, chunk.end);

            if (chunks.TryGetValue("TXTR", out chunk)) ReadList(textures, r, chunk.start, chunk.end);// textures
            if (chunks.TryGetValue("SPRT", out chunk)) ReadList(sprites, r, chunk.start, chunk.end);// sprites
            if (chunks.TryGetValue("OBJT", out chunk)) ReadList(objects, r, chunk.start, chunk.end);// sprites
            if (chunks.TryGetValue("BGND", out chunk)) ReadList(backgrounds, r, chunk.start, chunk.end);// backgrounds
            if (chunks.TryGetValue("SOND", out chunk)) ReadList(sounds, r, chunk.start, chunk.end);// audio files
            if (chunks.TryGetValue("AUDO", out chunk)) ReadList(rawAudio, r, chunk.start, chunk.end);// audio files

            if (chunks.TryGetValue("FONT", out chunk)) ReadList(fonts, r, chunk.start, chunk.end);// audio files
            if (chunks.TryGetValue("ROOM", out chunk)) ReadList(rooms, r, chunk.start, chunk.end);// sprites

            DebugPring();
            loaded = true;
        }
        public static void LoadResrouces(string data_win_filename)
        {
            if (loaded) return;
            LoadResrouces(new StreamReader(data_win_filename).BaseStream);
        }
    }
}