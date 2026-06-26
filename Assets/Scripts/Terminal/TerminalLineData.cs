using UnityEngine;

public enum TerminalLineType
{
    System,    // hijau gelap — pesan OS
    Input,     // hijau terang — yang player ketik
    Response,  // hijau normal — balasan sistem
    Error,     // red — pesan error/pelanggaran
    Warning    // yellow — peringatan
}

/// <summary>
/// Data model untuk satu baris teks di terminal.
/// </summary>
[System.Serializable]
public class TerminalLineData
{
    public string textContentString;
    public TerminalLineType lineTypeEnum;

    public TerminalLineData(string text, TerminalLineType lineType)
    {
        textContentString = text;
        lineTypeEnum      = lineType;
    }
}