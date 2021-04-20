using System;
using System.Globalization;
using nUpdate.Updating;

namespace Orcus.Server.Utilities
{
    public static class Updater
    {
        public static UpdateManager GetUpdateManager()
        {
            return new UpdateManager(new Uri("https://www.orcus.pw/orcusapp/update/servers/gui/updates.json"),
                "<RSAKeyValue><Modulus>vEqURFNh66jHWE1jq+ZnUxXMKvZpIWuqCyzBylljeX4Lj3yC8r2sc9bHPGU+izU8sTGFKPqUzdonV3zyTQ0VyTISUzxTodMMmFB1FZd/Y+09BmIIDvDSwO2N/VUYTmJRLPfy+09FAYo8SmV0GJSCLPokGu02Che9QTAFlAOru69eq0V+KfIaj3GRaIQGNeEseHwoxY6i3qt11hq3pD/zx30hkhuDrOW0QDG5vnAPIh9XVqHcX98Mz5+qVXmchzyv/hicBbbqTjS5kYBfohYhdPmvYGRD+kP/WclaeMS7i48Af/09fw7d4g3huP4ExbOMk91Gbo3+e2hs3DKB2w1ZhynrJwJgyQD1tERaqbIY/CGNZWx5jORulyuyzB6VSlgg+6ZMzMj8joKjF49/51ksXH9n8+19wu9EFG5ABuNCXJavNgEEDubRMEcoRR27J6n2PiOTQOo4dvZZ/qYSuBIrcyaQQq63dr2ELliPNeLCM/Um2GbNMoHhCZUF17hVAp+eoSmW4G+B1bhkITHVTJAfL24a3sFsVNhoJRwulmvALMkYdKWgg0jEkaexro3SzqCrytnUeGLD6v1teLw9qbP4a3LBglwjchM54rI6Afy7ZkI7eva7q1cMuCzp0vZPN64EZSruBAP3b7XAfu6qbsDJ+zJkrubQowM9Dy30SABZ/lXIv7rIdWT4h0oSxOe2gctzjcZuTBnhcYkASkl+i4xJH0vnaORwdKOvdR/yEJf8fJZn4UMB8fFn4tNSyTz3bwB7KgyOTmbEGVbtfbfc/u2d+LsMLJ04sZkv7zfLhkXQOtESBZvx5lyLCTKdiAAGlrE3Ndkky/1dOpcpBtDFuo6iE3idHBuQs6fRLMTxd+NFWIlrcQ3VkJ2wXCMEcUDU3LPFg+6+zU3iZH19DyQwCUBAfngbXVJQcEK7CUnoLIUDnt6a2D+BkKoBxVJV+yO6QT/Ppv2wE7NY81LOEoN5GfSn3lynrKL6ju5BuG8RZaTI+PZ6tA4i0JAVr/AFpPszYrMjg5UQVDqSB/2bQGjc/FcL7VWAIxl5HwMJHHPK4YJZDnBRRHmyLtaFDSrf/KIhDYpq3dfsMwLXr08l7ArgK53Wv11OhCdxYzOxqjbkGLOX82a70KM31RNgA+j1TYhFYmIcRs6tGuydJmb01eQqAdjHuCxnLwPwoJGXtkA4InO8lJDXVsQucxqXf9PKGdQCNbXXF3mJ08fJltFyerofp/QhBPeBdttE4kq6o7umjD6dtivUMVXMi5OHv6KiBLaA/8tnHYEtfcObMoYDydeOoVxMm9qKfs0rEfkwPslbp9lvONjINB5opLKa5MovqStTuRN/EOjZk+UmTip+cv00vTX4EQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
                new CultureInfo("en"), null)
            {CloseHostApplication = true};
        }
    }
}