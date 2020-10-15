using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Services;
using Microsoft.AspNetCore.Authorization;

namespace Private_WebApi.Controllers
{
    [Route("images")]
    public class ImagesController : ControllerBase
    {
        [HttpGet("{type}/{guid}")]
        public IActionResult Get(string type, string guid)
        {
            string imgDir, imgFileName, imgPath;
            imgFileName = guid + ".jpg";
            imgDir = CoreSettings.b2bDataLocalDir;
            switch (type)
            {
                case "topcategory":
                    imgDir += CoreSettings.TopCategoriesPicturePath + "/";
                    break;
                case "supplier":
                    imgDir += CoreSettings.SuppliersPicturePath + "/";
                    break;
                default:
                    return BadRequest();
            }

            imgPath = imgDir + imgFileName;
            if (!System.IO.File.Exists(imgPath))
            {
                imgPath = imgDir + "noimage.jpg";
            }
            return PhysicalFile(imgPath, "image/jpeg");
        }

        [HttpGet("product/{type}/{guid}")]
        public IActionResult GetProductImage(string type, string guid)
        {
            string imgDir, imgFileName, imgPath;
            imgFileName = guid + ".jpg";
            imgDir = CoreSettings.b2bDataLocalDir;
            switch (type)
            {
                case "matched":
                    imgDir += CoreSettings.MatchedProductsPicturesPath + "/";
                    break;
                case "unmatched":
                    imgDir += CoreSettings.UnmatchedProductsPicturesPath + "/";
                    break;
                case "conflicted":
                    imgDir += CoreSettings.ConflictedProductsPicturesPath + "/";
                    break;
                default:
                    return BadRequest();
            }

            imgPath = imgDir + imgFileName;
            if (!System.IO.File.Exists(imgPath))
            {
                imgPath = CoreSettings.b2bDataLocalDir + CoreSettings.ProductsPicturesPath + "/noimage.jpg";
            }
            return PhysicalFile(imgPath, "image/jpeg");

        }
    }
}
