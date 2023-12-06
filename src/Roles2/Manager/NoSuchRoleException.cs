using System;

namespace Lotus.Roles2.Manager;

public class NoSuchRoleException: Exception
{
    public NoSuchRoleException(string message): base(message) {}
}