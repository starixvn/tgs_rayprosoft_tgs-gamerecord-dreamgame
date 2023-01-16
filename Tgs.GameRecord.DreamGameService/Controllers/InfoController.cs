using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.DreamGameService.Core;

namespace Tgs.GameRecord.DreamGameService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public InfoController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet]
        public object Get()
        {
            var gameRecordInfo = _configuration.GetSection("GameRecordInfo").Get<GameRecordInfoOptions>();
            var mongoSetting = _configuration.GetSection("MongoSetting").Get<MongoSettingOptions>();
            var natsSetting = _configuration.GetSection("NatsSetting").Get<NatsSettingOptions>();

            return new
            {
                gameRecordInfo,
                mongoSetting,
                natsSetting
            };
        }

        [HttpGet("TF")]
        public object GetTF()
        {
            var gameRecordInfo = _configuration.GetSection("DGTFGameRecordInfo").Get<DGTFGameRecordInfoOptions>();
            var mongoSetting = _configuration.GetSection("DGTFMongoSetting").Get<DGTFMongoSettingOptions>();
            var natsSetting = _configuration.GetSection("DGTFNatsSetting").Get<DGTFNatsSettingOptions>();

            return new
            {
                gameRecordInfo,
                mongoSetting,
                natsSetting
            };
        }
    }
}
