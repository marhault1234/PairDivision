using AWSLambda1.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AWSLambda1.Settings
{
    public interface IPlayerSelect
    {
        List<Player> playerSelect(TeamSettingEntity teamSettingEntity);
    }
}
