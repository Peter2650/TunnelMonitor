
using System;
using System.ComponentModel;

public class PersonEntry : INotifyPropertyChanged
{
    private string? name;
    private string? phone;
    private string? company;
    private int numberOfPersons;
    private bool isTunnel1Checked;
    private bool isTunnel2Checked;
    private DateTime entryTime;
    private DateTime expectedReturnTime;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string Name
    {
        get => name ?? "";
        set
        {
            name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public string Phone
    {
        get => phone ?? "";
        set
        {
            phone = value;
            OnPropertyChanged(nameof(Phone));
        }
    }

    public string Company
    {
        get => company ?? "";
        set
        {
            company = value;
            OnPropertyChanged(nameof(Company));
        }
    }

    public int NumberOfPersons
    {
        get => numberOfPersons;
        set
        {
            numberOfPersons = value;
            OnPropertyChanged(nameof(NumberOfPersons));
        }
    }
    public bool IsTunnel1Checked
    {
        get => isTunnel1Checked;
        set
        {
            isTunnel1Checked = value;
            tunnel1 = value ? "X" : "";
        }
    }
    public bool IsTunnel2Checked
    {
        get => isTunnel2Checked;
        set
        {
            isTunnel2Checked = value;
            tunnel2 = value ? "X" : "";
        }
    }
    public string tunnel1 { get; set; } = "";
    public string tunnel2 { get; set; } = "";

    public DateTime EntryTime
    {
        get => entryTime;
        set
        {
            entryTime = value;
            OnPropertyChanged(nameof(EntryTime));
        }
    }

    public DateTime ExpectedReturnTime
    {
        get => expectedReturnTime;
        set
        {
            expectedReturnTime = value;
            OnPropertyChanged(nameof(ExpectedReturnTime));
        }
    }
    public string FileName { get; private set; }

    public PersonEntry(
        string name,
        string phone,
        string company,
        int numberOfPersons,
        bool isTunnel1Checked,
        bool isTunnel2Checked,
        DateTime entryTime,
        DateTime expectedReturn,
        string? fileName = null
    )
    {
        Name =
            name ?? throw new ArgumentNullException(nameof(name), "Navn kan ikke være null.");
        Phone =
            phone
            ?? throw new ArgumentNullException(nameof(phone), "Telefon kan ikke være null.");
        Company =
            company
            ?? throw new ArgumentNullException(nameof(company), "Firma kan ikke være null.");

        NumberOfPersons = numberOfPersons;
        IsTunnel1Checked = isTunnel1Checked;
        IsTunnel2Checked = isTunnel2Checked;

        EntryTime = entryTime;
        ExpectedReturnTime = expectedReturn;
        if (!string.IsNullOrEmpty(fileName))
        {
            FileName = fileName;
        }
        else
        {
            FileName = GenerateFileName();
        }
    }

    // Metode til at generere et unikt filnavn for personentry
    private string GenerateFileName()
    {
        return $"{Name}_{Phone}_{EntryTime:yyyyMMddHHmmss}.txt";
    }

    // Metode til at returnere en string repræsentation af personentry, for logning
    public override string ToString()
    {
        return $"{Name}, {Phone}, {Company}, # persons: {NumberOfPersons}, Tunnel1: {tunnel1}, Tunnel2: {tunnel2} Entered: {EntryTime}, Expected Return: {ExpectedReturnTime}";
    }

    private bool _isOverdue;
    public bool IsOverdue
    {
        get { return _isOverdue; }
        set
        {
            if (_isOverdue != value)
            {
                _isOverdue = value;
                OnPropertyChanged(nameof(IsOverdue));
            }
        }
    }
}