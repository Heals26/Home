using System.Reflection;

namespace Home.WebUI.Infrastructure.Enumerations;

public class BaseEnumeration : IComparable
{

    #region Constructors

    protected BaseEnumeration() { }

    protected BaseEnumeration(string name, long value)
    {
        this.Name = name;
        this.Value = value;
    }

    #endregion Constructors

    #region Properties

    public string Name { get; }

    public long Value { get; }

    #endregion Properties

    #region Methods

    public int CompareTo(object other)
        => this.Value.CompareTo(((BaseEnumeration)other).Value);

    public override bool Equals(object obj)
    {
        var _Enumeration = obj as BaseEnumeration;
        if (_Enumeration == null)
            return false;

        var _TypeMatches = this.GetType().Equals(obj.GetType());
        var _ValueMatches = this.Value.Equals(_Enumeration.Value);

        return _TypeMatches && _ValueMatches;
    }
    public static T? FromName<T>(string name) where T : BaseEnumeration
        => name != null ? GetAll<T>()?.SingleOrDefault(vt => vt.Name == name) : null;

    public static T? FromValue<T>(long value) where T : BaseEnumeration
        => GetAll<T>()?.SingleOrDefault(vt => vt.Value == value);

    public static IEnumerable<T> GetAll<T>() where T : BaseEnumeration
    {
        var _Fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        return _Fields.Select(f => f.GetValue(null)).Cast<T>();
    }

    public override int GetHashCode()
        => this.Value.GetHashCode();

    public static implicit operator long(BaseEnumeration enumeration)
        => enumeration.Value;

    public override string ToString()
        => this.Name;

    #endregion Methods

}
