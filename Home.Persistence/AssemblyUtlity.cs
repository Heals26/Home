using System.Reflection;

namespace Home.Persistence;

public class AssemblyUtility
{

    #region Methods

    public static Assembly GetAssembly() => typeof(AssemblyUtility).Assembly;

    #endregion Methods

}
