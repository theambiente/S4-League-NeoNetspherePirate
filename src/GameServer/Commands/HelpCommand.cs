using NeoNetsphere;
using NeoNetsphere.Commands;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Serializers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal class HelpCommand : ICommand
{
    public string Name { get; }
    public bool AllowConsole { get; }
    public SecurityLevel Permission { get; }
    public IReadOnlyList<ICommand> SubCommands { get; }

    // Todo
    public HelpCommand()
    {
        Name = "/help";
        AllowConsole = false;
        Permission = SecurityLevel.GameMaster;
        SubCommands = new ICommand[] { };
    }

    public async Task<bool> Execute(GameServer server, Player plr, string[] args)
    {
        plr.SendConsoleMessage(S4Color.Green + ">>>> Game Master Help System <<<<<");
        plr.SendConsoleMessage(S4Color.Green + ">>>> For every command, you have to whisper to \"server\"");
        plr.SendConsoleMessage(S4Color.Green + "> /whole_notice (Sends a message to the whole server)");
        plr.SendConsoleMessage(S4Color.Green + "> admin rename/playerkick/roomkick/addap/addpen/additem/rename");
        plr.SendConsoleMessage(S4Color.Green + "> /ban (for more infos just /ban) (Rank >2<)");
        plr.SendConsoleMessage(S4Color.Green + "> /search nickname after this press f11");
        plr.SendConsoleMessage(S4Color.Green + "> setmaster/roomkick/notice/");
        plr.SendConsoleMessage(S4Color.Green + "> /clan forcejoin;forcekick;setstaff;removestaff");

        return true;
    }

    public string Help()
    {
        var sb = new StringBuilder();
        sb.AppendLine(Name);
        foreach (var cmd in SubCommands)
        {
            sb.Append("");
            sb.AppendLine(cmd.Help());
        }

        return sb.ToString();
    }
}
