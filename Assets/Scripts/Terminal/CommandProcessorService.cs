using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Memproses semua command yang diketik player di terminal.
/// Tidak extends MonoBehaviour — ini plain C# class (service).
/// Tambah command baru dengan tambah method dan daftarkan di ProcessCommand().
/// </summary>
public class CommandProcessorService
{
    // Subject yang sedang aktif menunggu verdict
    private SubjectDataModel activeSubjectDataModel = null;

    // Jumlah verifikasi benar dalam shift ini
    private int correctVerdictCountInt = 0;

    // Database subject — nanti bisa diganti load dari JSON/ScriptableObject
    private List<SubjectDataModel> subjectDatabaseList = new List<SubjectDataModel>()
    {
        new SubjectDataModel("S-0042", "Ahmad Sutrisno",       "1987-03-14", "2027-03-14", false),
        new SubjectDataModel("S-0043", "[DATA CORRUPTED]",     "2031-??-??", "2019-00-00", true),
        new SubjectDataModel("S-0044", "Sari Dewi Lestari",    "1995-11-02", "2028-11-02", false),
    };

    // Delegate agar CommandProcessor bisa kirim baris ke terminal
    // tanpa perlu tahu apa itu TerminalController
    public delegate void OnLineAddedDelegate(string text, TerminalLineType lineType);
    private OnLineAddedDelegate onLineAddedCallback;

    public CommandProcessorService(OnLineAddedDelegate lineCallback)
    {
        onLineAddedCallback = lineCallback;
    }

    // ─────────────────────────────────────────────
    // Entry point — dipanggil TerminalController
    // ─────────────────────────────────────────────

    public void ProcessCommand(string rawInputString)
    {
        string trimmedInputString = rawInputString.Trim().ToLower();
        string[] inputPartsArray  = trimmedInputString.Split(' ');
        string baseCommandString  = inputPartsArray[0];

        // Tampilkan dulu apa yang player ketik
        AddLine("VERIFIER@ALPHA-SEC:~$ " + rawInputString, TerminalLineType.Input);

        if (string.IsNullOrWhiteSpace(trimmedInputString)) return;

        switch (baseCommandString)
        {
            case "help":    ExecuteHelpCommand();                     break;
            case "status":  ExecuteStatusCommand();                   break;
            case "fetch":   ExecuteFetchCommand(inputPartsArray);     break;
            case "approved":ExecuteVerdictCommand(isApproved: true);  break;
            case "denied":  ExecuteVerdictCommand(isApproved: false); break;
            case "clear":   ExecuteClearCommand();                    break;
            default:        ExecuteUnknownCommand(baseCommandString); break;
        }
    }

    // ─────────────────────────────────────────────
    // Command implementations
    // ─────────────────────────────────────────────

    private void ExecuteHelpCommand()
    {
        AddLine("=== AVAILABLE COMMANDS ===",               TerminalLineType.System);
        AddLine("help           — daftar perintah",         TerminalLineType.Response);
        AddLine("status         — cek status sistem",       TerminalLineType.Response);
        AddLine("fetch [id]     — ambil file subject",      TerminalLineType.Response);
        AddLine("approved       — setujui subject aktif",   TerminalLineType.Response);
        AddLine("denied         — tolak subject aktif",     TerminalLineType.Response);
        AddLine("clear          — bersihkan layar",         TerminalLineType.Response);
        AddLine("==========================",               TerminalLineType.System);
    }

    private void ExecuteStatusCommand()
    {
        bool hasPendingSubjectBool = activeSubjectDataModel != null;

        AddLine("SYSTEM STATUS REPORT",                                              TerminalLineType.System);
        AddLine("> Koneksi database  : [OK]",                                        TerminalLineType.Response);
        AddLine("> Printer           : [STANDBY]",                                   TerminalLineType.Response);
        AddLine("> Shift akurasi     : " + correctVerdictCountInt + " benar",        TerminalLineType.Response);
        AddLine("> Subject aktif     : " + (hasPendingSubjectBool ? activeSubjectDataModel.subjectIdString : "tidak ada"),
                                                                                     TerminalLineType.Response);
        if (hasPendingSubjectBool)
            AddLine("> Menunggu verdict untuk: " + activeSubjectDataModel.subjectIdString, TerminalLineType.Warning);
    }

    private void ExecuteFetchCommand(string[] inputPartsArray)
    {
        if (inputPartsArray.Length < 2)
        {
            AddLine("ERR: format salah. Gunakan: fetch [ID]", TerminalLineType.Error);
            return;
        }

        string requestedIdString = inputPartsArray[1].ToUpper();
        SubjectDataModel foundSubject = subjectDatabaseList.Find(
            subject => subject.subjectIdString == requestedIdString
        );

        if (foundSubject == null)
        {
            AddLine("ERR: ID tidak ditemukan — " + requestedIdString, TerminalLineType.Error);
            return;
        }

        activeSubjectDataModel = foundSubject;

        AddLine(">>> FILE DITERIMA <<<",                             TerminalLineType.System);
        AddLine("================================",                  TerminalLineType.Response);
        AddLine("SUBJECT ID   : " + foundSubject.subjectIdString,   TerminalLineType.Response);
        AddLine("NAMA         : " + foundSubject.fullNameString,     TerminalLineType.Response);
        AddLine("TGL LAHIR    : " + foundSubject.dateOfBirthString,  TerminalLineType.Response);
        AddLine("EXP DATE     : " + foundSubject.expiryDateString,   TerminalLineType.Response);
        AddLine("================================",                  TerminalLineType.Response);
        AddLine("> Periksa data. Ketik approved atau denied.",       TerminalLineType.Warning);
    }

    private void ExecuteVerdictCommand(bool isApproved)
    {
        if (activeSubjectDataModel == null)
        {
            AddLine("ERR: tidak ada subject aktif. Gunakan fetch [ID] dulu.", TerminalLineType.Error);
            return;
        }

        string verdictString    = isApproved ? "APPROVED" : "DENIED";
        bool isCorrectVerdict   = isApproved != activeSubjectDataModel.isMimicBool;

        AddLine(">>> VERDICT: " + verdictString + " <<<", TerminalLineType.System);

        if (isCorrectVerdict)
        {
            AddLine("[BENAR] Verifikasi akurat.", TerminalLineType.Response);
            AddLine("Dokumen dicetak dan dikirim.", TerminalLineType.Response);
            correctVerdictCountInt++;
        }
        else
        {
            if (activeSubjectDataModel.isMimicBool && isApproved)
                AddLine("[FATAL] Kamu menyetujui MIMIC. Alpha Sector terancam bahaya.", TerminalLineType.Error);
            else
                AddLine("[ERROR] Kamu menolak warga sah. Laporan dibuat.", TerminalLineType.Error);
        }

        // Reset subject aktif setelah verdict
        activeSubjectDataModel = null;
    }

    private void ExecuteClearCommand()
    {
        // Kirim sinyal khusus ke terminal untuk clear layar
        AddLine("__CLEAR__", TerminalLineType.System);
    }

    private void ExecuteUnknownCommand(string unknownCommandString)
    {
        AddLine("ERR: perintah tidak dikenal — \"" + unknownCommandString + "\"", TerminalLineType.Error);
        AddLine("> Ketik help untuk daftar perintah.", TerminalLineType.System);
    }

    // ─────────────────────────────────────────────
    // Helper
    // ─────────────────────────────────────────────

    private void AddLine(string textString, TerminalLineType lineTypeEnum)
    {
        onLineAddedCallback?.Invoke(textString, lineTypeEnum);
    }
}