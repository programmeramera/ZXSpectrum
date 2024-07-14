using System;
using System.Text;

namespace Zilog;

public abstract partial class Z80
{

    public void WriteByteToMemoryOverridden(int address, byte b)
    {
        this.WriteByteToMemory((ushort)address, b);
    }

    public Z80()
    {
        //Initiate Parity table
        for (int a = 0; a < 256; a++)
        {
            bool p = true;
            for (int b = 0; b < 8; b++)
            {
                if ((a & (1 << b)) != 0)
                {
                    p = !p;
                }
            }
            Parity[a] = p;
        }
    }

    public virtual void TstateChange(int diff)
    {
    }

    #region Ports
    //Override these
    public virtual int In(int port)
    {
        return 0xff;
    }

    public virtual void Out(int Port, int ByteValue, int tStates)
    {
    }

    #endregion

    #region Registers and access to them

    #region Flags
    //Flags
    private bool fS = false;
    public bool fZ = false;
    private bool f5 = false;
    private bool fH = false;
    private bool f3 = false;
    private bool fPV = false;
    private bool fN = false;
    private bool fC = false;

    public int F
    {
        get
        {
            return (fS ? F_S : 0) |
                (fZ ? F_Z : 0) |
                (f5 ? F_5 : 0) |
                (fH ? F_H : 0) |
                (f3 ? F_3 : 0) |
                (fPV ? F_PV : 0) |
                (fN ? F_N : 0) |
                (fC ? F_C : 0);
        }
        set
        {
            fS = (value & F_S) != 0;
            fZ = (value & F_Z) != 0;
            f5 = (value & F_5) != 0;
            fH = (value & F_H) != 0;
            f3 = (value & F_3) != 0;
            fPV = (value & F_PV) != 0;
            fN = (value & F_N) != 0;
            fC = (value & F_C) != 0;
        }
    }

    private static int F_C = 0x01;
    private static int F_N = 0x02;
    private static int F_PV = 0x04;
    private static int F_3 = 0x08;
    private static int F_H = 0x10;
    private static int F_5 = 0x20;
    private static int F_Z = 0x40;
    private static int F_S = 0x80;

    #endregion

    //Registers
    private static int R_A = 0x07;
    private static int R_B = 0x00;
    private static int R_C = 0x01;
    private static int R_D = 0x02;
    private static int R_E = 0x03;
    private static int R_H = 0x04;
    private static int R_L = 0x05;
    //Index Registers
    public ushort[] IndexRegistry = new ushort[2];
    public ushort IX
    {
        get { return IndexRegistry[(int)IndexRegistryEnum.IX]; }
        set { IndexRegistry[(int)IndexRegistryEnum.IX] = value; }
    }
    public ushort IY
    {
        get { return IndexRegistry[(int)IndexRegistryEnum.IY]; }
        set { IndexRegistry[(int)IndexRegistryEnum.IY] = value; }
    }

    //Interrupt Register
    public byte I = 0;
    public byte[] Registers = new byte[10];

    //Registers Prim
    public byte[] RegitersPrim = new byte[10];
    public byte Fprim = 0;
    //8bit
    public byte A
    {
        get { return Registers[R_A]; }
        set { Registers[R_A] = value; }
    }

    public byte B
    {
        get { return Registers[R_B]; }
        set { Registers[R_B] = value; }
    }

    public byte C
    {
        get { return Registers[R_C]; }
        set { Registers[R_C] = value; }
    }

    public byte D
    {
        get { return Registers[R_D]; }
        set { Registers[R_D] = value; }
    }

    public byte E
    {
        get { return Registers[R_E]; }
        set { Registers[R_E] = value; }
    }

    public byte H
    {
        get { return Registers[R_H]; }
        set { Registers[R_H] = value; }
    }

    public byte L
    {
        get { return Registers[R_L]; }
        set { Registers[R_L] = value; }
    }

    public byte APrim
    {
        get { return RegitersPrim[R_A]; }
        set { RegitersPrim[R_A] = value; }
    }

    public byte BPrim
    {
        get { return RegitersPrim[R_B]; }
        set { RegitersPrim[R_B] = value; }
    }

    public byte CPrim
    {
        get { return RegitersPrim[R_C]; }
        set { RegitersPrim[R_C] = value; }
    }

    public byte DPrim
    {
        get { return RegitersPrim[R_D]; }
        set { RegitersPrim[R_D] = value; }
    }

    public byte EPrim
    {
        get { return RegitersPrim[R_E]; }
        set { RegitersPrim[R_E] = value; }
    }

    public byte HPrim
    {
        get { return RegitersPrim[R_H]; }
        set { RegitersPrim[R_H] = value; }
    }

    public byte LPrim
    {
        get { return RegitersPrim[R_L]; }
        set { RegitersPrim[R_L] = value; }
    }

    public byte FPrim
    {
        get { return Fprim; }
        set { Fprim = value; }
    }

    //16 bit
    public ushort HL
    {
        get
        {
            return (ushort)(Registers[R_H] << 8 | Registers[R_L]);
        }
        set
        {
            Registers[R_H] = (byte)(value >> 8);
            Registers[R_L] = (byte)(value & 0xff);
            //if (value == 23672)
            //{
            //    Console.WriteLine("SET hl:" + HL.ToString());
            //    Console.WriteLine("PC: " + PC.ToString());
            //    Console.WriteLine("OP: 0x" + opcode.ToString("x"));
            //}
        }
    }

    public ushort HLPrim
    {
        get
        {
            return (ushort)(RegitersPrim[R_H] << 8 | RegitersPrim[R_L]);
        }
        set
        {
            RegitersPrim[R_H] = (byte)(value >> 8);
            RegitersPrim[R_L] = (byte)(value & 0xff);
        }
    }

    public ushort DE
    {
        get
        {
            return (ushort)(Registers[R_D] << 8 | Registers[R_E]);
        }
        set
        {
            Registers[R_D] = (byte)(value >> 8);
            Registers[R_E] = (byte)(value & 0xff);
        }
    }

    public ushort DEPrim
    {
        get
        {
            return (ushort)(RegitersPrim[R_D] << 8 | RegitersPrim[R_E]);
        }
        set
        {
            RegitersPrim[R_D] = (byte)(value >> 8);
            RegitersPrim[R_E] = (byte)(value & 0xff);
        }
    }

    public ushort BC
    {
        get
        {
            return (ushort)(Registers[R_B] << 8 | Registers[R_C]);
        }
        set
        {
            Registers[R_B] = (byte)(value >> 8);
            Registers[R_C] = (byte)(value & 0xff);
        }
    }

    public ushort BCPrim
    {
        get
        {
            return (ushort)(RegitersPrim[R_B] << 8 | RegitersPrim[R_C]);
        }
        set
        {
            RegitersPrim[R_B] = (byte)(value >> 8);
            RegitersPrim[R_C] = (byte)(value & 0xff);
        }
    }

    public ushort AF
    {
        get
        {
            return (ushort)(Registers[R_A] << 8 | F);
        }
        set
        {
            Registers[R_A] = (byte)(value >> 8);
            F = value & 0xff;
        }
    }

    public ushort AFPrim
    {
        get
        {
            return (ushort)(RegitersPrim[R_A] << 8 | FPrim);
        }
        set
        {
            RegitersPrim[R_A] = (byte)(value >> 8);
            Fprim = (byte)(value & 0xff);
        }
    }

    public byte IXL
    {
        get { return (byte)(IX & 0xff); }
        set { IX = (byte)((IX & 0xff00) | (value)); }
    }

    public byte IXH
    {
        get { return (byte)((IX >> 8) & 0xff); }
        set { IX = (byte)((IX & 0xff) | (value << 8)); }
    }

    public byte IYL
    {
        get { return (byte)(IY & 0xff); }
        set { IY = (byte)((IY & 0xff00) | (value)); }
    }

    public byte IYH
    {
        get { return (byte)((IY >> 8) & 0xff); }
        set { IY = (byte)((IY & 0xff) | (value << 8)); }
    }

    /// <summary>
    /// Returns the value in register accoording to current opcode
    /// </summary>
    /// <param name="rpos"></param>
    /// <returns></returns>
    public byte RegisterValueFromOP(int rpos)
    {
        return Registers[(opcode >> rpos) & 7];
    }

    public int BitValueFromOP
    {
        get { return ((opcode >> 3) & 7); }
    }

    private bool d
    {
        get { return Sign(ReadByteFromMemory(PC++)); }
    }

    #endregion

    #region Program Counter
    //PC Stack call
    public void PCToStack()
    {
        StackpushWord(PC);
    }
    public void PCFromStack()
    {
        PC = StackpopWord();
    }

    protected byte GetNextPCByte()
    {
        byte b = ReadByteFromMemory(PC);
        PC = (ushort)(PC + 1 & 0xffff);
        return b;
    }
    protected ushort GetNextPCWord()
    {
        ushort w = ReadByteFromMemory(PC++);
        w |= (ushort)(ReadByteFromMemory((ushort)(PC++ & 0xffff)) << 8);
        return w;
    }
    #endregion

    #region Stack
    public void StackpushWord(ushort word)
    {
        ushort sp = (ushort)((SP - 2) & 0xffff);
        SP = sp;
        WriteWordToMemory(sp, word);
    }
    public ushort StackpopWord()
    {
        ushort w = ReadByteFromMemory(SP);
        SP++;
        w |= (ushort)(ReadByteFromMemory((ushort)(SP & 0xffff)) << 8);
        SP = (ushort)(SP + 1 & 0xffff);
        return w;
    }
    #endregion

    #region Memory
    public abstract void WriteWordToMemory(ushort address, ushort word);

    public abstract void WriteByteToMemory(ushort address, byte bytetowrite);

    public abstract byte ReadByteFromMemory(ushort address);

    public ushort ReadWordFromMemory(ushort address)
    {
        return (ushort)((ReadByteFromMemory((ushort)(address + 1 & 0xffff)) << 8 | ReadByteFromMemory((ushort)(address & 0xffff))) & 0xffff);
    }
    #endregion

    //Program Counter
    public ushort PC;
    //Stack Pointer
    public ushort SP = 0xffff;
    protected ushort opcode = 0;
    protected int _numberOfTStatesLeft;
    // int diff = 0;
    public void SubtractNumberOfTStatesLeft(int tstates = 0)
    {
        _numberOfTStatesLeft -= tstates;
        TstateChange(tstates);
    }
    public bool[] Parity = new bool[256];

    protected int _EndTstates2 = 0;
    public int EndTstates2
    {
        get
        {
            return _EndTstates2 + (_numberOfTStatesLeft * -1);
        }
    }
    //Interupts and memory
    public bool BlockINT = true;
    public bool IFF = false;
    public bool IFF2 = false;
    public int IM = 2;
    public int _R7 = 0;
    public int _R = 0;
    public int R7
    {
        get { return _R7; }
        set { _R7 = value; }
    }

    public byte R
    {
        get { return (byte)((_R & 0x7f) | _R7); }
        set
        {
            _R = value;
            _R7 = value & 0x80;
        }
    }

    public void Refresh(int t)
    {
        //            _R += t;
        _R = (byte)(((_R + 1) & 0x7F) | (_R & 0x80));
        //SubtractNumberOfTStatesLeft( 1;
    }

    public void Reset()
    {
        PC = 0;
        SP = 0;

        A = 0;
        F = 0;
        BC = 0;
        DE = 0;
        HL = 0;

        EXX();
        AFPrim = 0;

        A = 0;
        F = 0;
        BC = 0;
        DE = 0;
        HL = 0;

        IX = 0;
        IY = 0;

        R = 0;

        I = 0;
        IFF = false;
        IFF2 = false;
        IM = 0;
        _numberOfTStatesLeft = 0;
        this.Out(254, 5, 0); //Border Color

        //Clear Memory
        for (int a = 16384; a < 3 * 16384; a++)
        {
            WriteByteToMemory((ushort)a, 0);
        }
    }

    //System.Text.StringBuilder sb = new StringBuilder();
    public void NextOpcode()
    {
        opcode = (ushort)(ReadByteFromMemory(PC) & 0xff);
        PC = (ushort)((PC + 1) & 0xffff);
    }

    public int interrupt()
    {
        // If it's not a masking interrupt
        if (!IFF)
        {
            return 0;
        }

        switch (IM)
        {
            case 1:
                PCToStack();
                IFF = false;
                IFF2 = false;
                PC = 56;
                return 13;
            case 2:
                PCToStack();
                IFF = false;
                IFF2 = false;
                var t = (ushort)((I << 8) | 0x00ff);
                PC = ReadWordFromMemory(t);
                return 19;          
        }

        return 0;
    }

    public int NumberOfTstates = 0;

    public StringBuilder sb = new StringBuilder();
    // private DateTime start;
    public void DoInstructions(int numberOfTStates = 69888)
    {
        DoInstructions(numberOfTStates, null);
    }

    public virtual void DoInstructions(int numberOfTStates, Func<Z80, int> gameSpecificFunc)
    {
        NumberOfTstates = numberOfTStates;
        _numberOfTStatesLeft += numberOfTStates;
        _EndTstates2 = numberOfTStates;
        while (true)
        {

            if (interruptTriggered(_numberOfTStatesLeft))
            {
                //NumberOfTStatesLeft += (NumberOfTStates - interrupt());
                SubtractNumberOfTStatesLeft(interrupt());
                break;
            }

            //Refresh(1);

            NextOpcode();
            if (gameSpecificFunc != null)
            {
                SubtractNumberOfTStatesLeft(gameSpecificFunc(this));
            }

            switch (opcode)
            {
                case 0xCB:
                    NextOpcode();
                    DoCBPrefixInstruction();
                    break;
                case 0xDD:
                    Refresh(1);
                    NextOpcode();
                    DoDDorFDPrefixInstruction(IndexRegistryEnum.IX);
                    break;
                case 0xED:
                    Refresh(1);
                    NextOpcode();
                    DoEDPrefixInstruction();
                    break;
                case 0xFD:
                    Refresh(1);
                    NextOpcode();
                    DoDDorFDPrefixInstruction(IndexRegistryEnum.IY);
                    break;
                default:
                    Refresh(1);
                    DoNoPrefixInstruction();
                    break;
            }
        }
    }
}
