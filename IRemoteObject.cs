using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mdock;

public interface IRemoteObject
{
    //void StartupNextInstance(string[] args);
    void StartupNextInstance(string? path, int pos);
}
