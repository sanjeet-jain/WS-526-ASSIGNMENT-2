using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ImageSharingWithModel.Models;

public class ListByUserView
{
    public int Id { get; set; }
    public IEnumerable<SelectListItem> Users { get; set; }
}