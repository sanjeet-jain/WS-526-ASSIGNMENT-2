using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ImageSharingWithModel.Models;

public class ListByTagViewModel
{
    public int Id { get; set; }
    public IEnumerable<SelectListItem> Tags { get; set; }
}