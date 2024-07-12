using System;
using System.Text;

namespace Zilog;

public abstract partial class Z80
{

    public void WriteByteToMemoryOverridden(int address, byte b)
    {
        this.WriteByteToMemory(address, b);
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
    public virtual byte In(int port)
    {
        return 0xff;
    }

    public virtual void Out(int Port, byte ByteValue, int tStates)
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

    public byte F
    {
        get
        {
            int result = fS ? F_S :0;
            result |= fZ ? F_Z : 0;
            result |= f5 ? F_5 : 0;
            result |= fH ? F_H : 0;
            result |= f3 ? F_3 : 0;
            result |= fPV ? F_PV : 0;
            result |= fN ? F_N : 0;
            result |= fC ? F_C : 0;
            return (byte)result;
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

    private static byte F_C = 0x01;
    private static byte F_N = 0x02;
    private static byte F_PV = 0x04;
    private static byte F_3 = 0x08;
    private static byte F_H = 0x10;
    private static byte F_5 = 0x20;
    private static byte F_Z = 0x40;
    private static byte F_S = 0x80;

    #endregion

    //Registers
    private static byte R_A = 0x07;
    private static byte R_B = 0x00;
    private static byte R_C = 0x01;
    private static byte R_D = 0x02;
    private static byte R_E = 0x03;
    private static byte R_H = 0x04;
    private static byte R_L = 0x05;
    //Index Registers
    public int[] IndexRegistry = new int[2];
    public int IX
    {
        get { return IndexRegistry[(int)IndexRegistryEnum.IX]; }
        set { IndexRegistry[(int)IndexRegistryEnum.IX] = value; }
    }
    public int IY
    {
        get { return IndexRegistry[(int)IndexRegistryEnum.IY]; }
        set { IndexRegistry[(int)IndexRegistryEnum.IY] = value; }
    }

    //Interrupt Register
    public byte I = 0;
    public byte[] Registers = new byte[10];

    //Registers Prim
    public byte[] RegistersPrim = new byte[10];
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
        get { return RegistersPrim[R_A]; }
        set { RegistersPrim[R_A] = value; }
    }

    public byte BPrim
    {
        get { return RegistersPrim[R_B]; }
        set { RegistersPrim[R_B] = value; }
    }

    public byte CPrim
    {
        get { return RegistersPrim[R_C]; }
        set { RegistersPrim[R_C] = value; }
    }

    public byte DPrim
    {
        get { return RegistersPrim[R_D]; }
        set { RegistersPrim[R_D] = value; }
    }

    public byte EPrim
    {
        get { return RegistersPrim[R_E]; }
        set { RegistersPrim[R_E] = value; }
    }

    public byte HPrim
    {
        get { return RegistersPrim[R_H]; }
        set { RegistersPrim[R_H] = value; }
    }

    public byte LPrim
    {
        get { return RegistersPrim[R_L]; }
        set { RegistersPrim[R_L] = value; }
    }

    public byte FPrim
    {
        get { return Fprim; }
        set { Fprim = value; }
    }

    //16 bit
    public int HL
    {
        get
        {
            return Registers[R_H] << 8 | Registers[R_L];
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

    public int HLPrim
    {
        get
        {
            return RegistersPrim[R_H] << 8 | RegistersPrim[R_L];
        }
        set
        {
            RegistersPrim[R_H] = (byte)(value >> 8);
            RegistersPrim[R_L] = (byte)(value & 0xff);
        }
    }

    public int DE
    {
        get
        {
            return Registers[R_D] << 8 | Registers[R_E];
        }
        set
        {
            Registers[R_D] = (byte)(value >> 8);
            Registers[R_E] = (byte)(value & 0xff);
        }
    }

    public int DEPrim
    {
        get
        {
            return RegistersPrim[R_D] << 8 | RegistersPrim[R_E];
        }
        set
        {
            RegistersPrim[R_D] = (byte)(value >> 8);
            RegistersPrim[R_E] = (byte)(value & 0xff);
        }
    }

    public int BC
    {
        get
        {
            return Registers[R_B] << 8 | Registers[R_C];
        }
        set
        {
            Registers[R_B] = (byte)(value >> 8);
            Registers[R_C] = (byte)(value & 0xff);
        }
    }

    public int BCPrim
    {
        get
        {
            return RegistersPrim[R_B] << 8 | RegistersPrim[R_C];
        }
        set
        {
            RegistersPrim[R_B] = (byte)(value >> 8);
            RegistersPrim[R_C] = (byte)(value & 0xff);
        }
    }

    public int AF
    {
        get
        {
            return Registers[R_A] << 8 | F;
        }
        set
        {
            Registers[R_A] = (byte)(value >> 8);
            F = (byte)(value & 0xff);
        }
    }

    public int AFPrim
    {
        get
        {
            return RegistersPrim[R_A] << 8 | FPrim;
        }
        set
        {
            RegistersPrim[R_A] = (byte)(value >> 8);
            Fprim = (byte)(value & 0xff);
        }
    }

    public byte IXL
    {
        get { return (byte)(IX & 0xff); }
        set { IX = (IX & 0xff00) | (value); }
    }

    public byte IXH
    {
        get { return (byte)((IX >> 8) & 0xff); }
        set { IX = (IX & 0xff) | (value << 8); }
    }

    public byte IYL
    {
        get { return (byte)(IY & 0xff); }
        set { IY = (IY & 0xff00) | (value); }
    }

    public byte IYH
    {
        get { return (byte)((IY >> 8) & 0xff); }
        set { IY = (IY & 0xff) | (value << 8); }
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

    public byte BitValueFromOP
    {
        get { return (byte)((opcode >> 3) & 7); }
    }

    private byte d
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
        PC = PC + 1 & 0xffff;
        return b;
    }
    protected int GetNextPCWord()
    {
        int w = ReadByteFromMemory(PC++);
        w |= ReadByteFromMemory(PC++ & 0xffff) << 8;
        return w;
    }
    #endregion

    #region Stack
    public void StackpushWord(int word)
    {
        int sp = ((SP - 2) & 0xffff);
        SP = sp;
        WriteWordToMemory(sp, word);
    }
    public int StackpopWord()
    {
        int w = ReadByteFromMemory(SP);
        SP++;
        w |= ReadByteFromMemory(SP & 0xffff) << 8;
        SP = (SP + 1 & 0xffff);
        return w;
    }
    #endregion

    #region Memory
    public abstract void WriteWordToMemory(int address, int word);

    public abstract void WriteByteToMemory(int address, byte bytetowrite);

    public abstract byte ReadByteFromMemory(int address);

    public int ReadWordFromMemory(int address)
    {
        return (ReadByteFromMemory(address + 1 & 0xffff) << 8 | ReadByteFromMemory(address & 0xffff)) & 0xffff;
    }
    #endregion

    //Program Counter
    public int PC;
    //Stack Pointer
    public int SP = 0x10000;
    protected int opcode = 0;
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
    public byte _R7 = 0;
    public byte _R = 0;
    public byte R7
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
            _R7 = (byte)(value & 0x80);
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
            WriteByteToMemory(a, 0);
        }
    }

    //System.Text.StringBuilder sb = new StringBuilder();
    public void NextOpcode()
    {
        opcode = (ReadByteFromMemory(PC) & 0xff);
        PC = (PC + 1) & 0xffff;
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
            case 0:
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
                int t = (I << 8) | 0x00ff;
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
