﻿namespace ProudNetSrc
{
  using System;
  using System.IO;
  using System.Security.Cryptography;
  using System.Threading;
  using BlubLib.DotNetty;
  using BlubLib.IO;
  using BlubLib.Security.Cryptography;
  using DotNetty.Buffers;
  using ReadOnlyByteBufferStream = BlubLib.DotNetty.ReadOnlyByteBufferStream;

  internal class Crypt : IDisposable
  {
    private static readonly byte[] S_defaultKey =
        {0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

    private int _decryptCounter;

    private int _encryptCounter;

    public Crypt(int keySize, int fastKeySize)
    {
      if (keySize == 0)
      {
        AES = new RijndaelManaged
        {
          BlockSize = S_defaultKey.Length * 8,
          KeySize = S_defaultKey.Length * 8,
          Padding = PaddingMode.None,
          Mode = CipherMode.ECB,
          Key = S_defaultKey
        };
      }
      else
      {
        AES = new RijndaelManaged
        {
          BlockSize = keySize,
          KeySize = keySize,
          Padding = PaddingMode.None,
          Mode = CipherMode.ECB
        };
        AES.GenerateKey();
      }

      if (fastKeySize == 0)
      {
        RC4 = new RC4
        {
          KeySize = S_defaultKey.Length * 8,
          Key = S_defaultKey
        };
      }
      else
      {
        RC4 = new RC4 { KeySize = fastKeySize };
        RC4.GenerateKey();
      }
    }
        //comehere
    public Crypt(byte[] secureKey)
    {
      AES = new RijndaelManaged
      {
        BlockSize = secureKey.Length * 8,
        KeySize = secureKey.Length * 8,
        Padding = PaddingMode.None,
        Mode = CipherMode.ECB,
        Key = secureKey
      };
    }

    public SymmetricAlgorithm AES { get; private set; }
    public SymmetricAlgorithm RC4 { get; private set; }

    public void Dispose()
    {
      if (AES != null)
      {
        AES.Dispose();
        AES = null;
      }

      if (RC4 != null)
      {
        RC4.Dispose();
        RC4 = null;
      }
    }

    internal void InitializeFastEncryption(byte[] key)
    {
      RC4 = new RC4
      {
        KeySize = key.Length * 8,
        Key = key
      };
    }

    public byte[] DecryptAES(byte[] input)
    {
      if (RC4 == null || AES == null)
        return new byte[0];

      var dst = new MemoryStream();

      using (var encryptor = AES.CreateEncryptor())
      using (var cs = new CryptoStream(new NonClosingStream(new MemoryStream(input)), encryptor,
          CryptoStreamMode.Read))
      using (var w = cs.ToBinaryWriter(false))
      {
        cs.CopyTo(dst);
        return dst.ToArray();
      }
    }

    public void Encrypt(IByteBufferAllocator allocator, EncryptMode mode, Stream src, Stream dst, bool reliable)
    {
      if (RC4 == null || AES == null)
        return;
      //throw new ObjectDisposedException(GetType().FullName);

      using (var data = new BufferWrapper(allocator.Buffer()))
      using (var encryptor = GetAlgorithm(mode).CreateEncryptor())
      using (var cs = new CryptoStream(new NonClosingStream(dst), encryptor, CryptoStreamMode.Write))
      using (var w = cs.ToBinaryWriter(false))
      {
        var blockSize = AES.BlockSize / 8;
        var padding = blockSize - ((src.Length + 1 + 4) % blockSize);
        if (reliable)
          padding = blockSize - ((src.Length + 1 + 4 + 2) % blockSize);

        if (reliable)
        {
          var counter = (ushort)(Interlocked.Increment(ref _encryptCounter) - 1);
          data.Buffer.WriteShortLE(counter);
        }

        using (var dataStream = new WriteOnlyByteBufferStream(data.Buffer, false))
        {
          src.CopyTo(dataStream);
        }

        w.Write((byte)padding);
        using (var dataStream = new ReadOnlyByteBufferStream(data.Buffer, false))
        {
          w.Write(Hash.GetUInt32<CRC32>(dataStream));
          dataStream.Position = 0;
          dataStream.CopyTo(cs);
        }

        w.Fill((int)padding);
      }
    }

    public void Decrypt(IByteBufferAllocator allocator, EncryptMode mode, Stream src, Stream dst, bool reliable)
    {
      if (RC4 == null || AES == null)
        return;
      //throw new ObjectDisposedException(GetType().FullName);

      using (var data = new BufferWrapper(allocator.Buffer()))
      using (var decryptor = GetAlgorithm(mode).CreateDecryptor())
      using (var cs = new CryptoStream(src, decryptor, CryptoStreamMode.Read))
      {
        var padding = cs.ReadByte();
        var checksum = cs.ReadByte() | (cs.ReadByte() << 8) | (cs.ReadByte() << 16) | (cs.ReadByte() << 24);

        using (var dataStream = new WriteOnlyByteBufferStream(data.Buffer, false))
        {
          cs.CopyTo(dataStream);
        }

        if (reliable)
        {
          var counter = (ushort)(Interlocked.Increment(ref _decryptCounter) - 1);
          var messageCounter = data.Buffer.GetShortLE(data.Buffer.ReaderIndex);

          if (counter != messageCounter)
            throw new ProudException($"Invalid decrypt counter! Remote: {messageCounter} Local: {counter}");
        }

        var slice = data.Buffer.ReadSlice(data.Buffer.ReadableBytes - padding);
        using (var dataStream = new ReadOnlyByteBufferStream(slice, false))
        {
          if (Hash.GetUInt32<CRC32>(dataStream) != (uint)checksum)
            throw new ProudException("Invalid checksum");

          dataStream.Position = reliable ? 2 : 0;
          dataStream.CopyTo(dst);
        }
      }
    }

    private SymmetricAlgorithm GetAlgorithm(EncryptMode mode)
    {
      switch (mode)
      {
        case EncryptMode.Fast:
          return RC4;

        case EncryptMode.Secure:
          return AES;

        default:
          throw new ArgumentException("Invalid mode", nameof(mode));
      }
    }

    private class BufferWrapper : IDisposable
    {
      public BufferWrapper(IByteBuffer buffer)
      {
        Buffer = buffer;
      }

      public IByteBuffer Buffer { get; private set; }

      public void Dispose()
      {
        Buffer?.Release();
        Buffer = null;
      }
    }
  }
}
