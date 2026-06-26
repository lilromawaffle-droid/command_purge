using System;

/// <summary>
/// Data satu subject yang harus diverifikasi player.
/// Tambah field baru di sini kalau butuh data tambahan (foto, sidik jari, dll).
/// </summary>
[Serializable]
public class SubjectDataModel
{
    public string subjectIdString;
    public string fullNameString;
    public string dateOfBirthString;
    public string expiryDateString;
    public bool   isMimicBool;

    public SubjectDataModel(
        string subjectId,
        string fullName,
        string dateOfBirth,
        string expiryDate,
        bool   isMimic)
    {
        subjectIdString    = subjectId;
        fullNameString     = fullName;
        dateOfBirthString  = dateOfBirth;
        expiryDateString   = expiryDate;
        isMimicBool        = isMimic;
    }
}