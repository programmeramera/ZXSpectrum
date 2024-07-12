namespace ZXBox.Hardware.Interfaces;

public interface IInput
{
    byte Input(int Port, int tact);
    void AddTStates(int tstates)
    { }
}
