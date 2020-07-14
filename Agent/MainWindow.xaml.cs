using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Agent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /*  List<Offer> offers = db.Offers
      .Include(of => of.Product)
      .Where(of => of.SupplierId == new Guid("fe9e2725-96f9-4d88-ad08-e010953a1dfd"))
      .ToList();
  //   XMLProcessor.RequestProductsDescription(offers, new Guid("fe9e2725-96f9-4d88-ad08-e010953a1dfd"));
  XMLProcessor.ProductsDescriptions(offers, new Guid("fe9e2725-96f9-4d88-ad08-e010953a1dfd"));*/

        private void btExtractAllOffers_Click(object sender, RoutedEventArgs e)
        {
           // XMLProcessor.ExtractAllProductsToXML("D://test.xml", new Guid("fe9e2725-96f9-4d88-ad08-e010953a1dfd"));
            using (MarketDbContext db = new MarketDbContext())
            {

                List <Supplier> suppliersList = db.Suppliers.ToList();
                foreach(Supplier supplier in suppliersList)
                {
                    XMLProcessor.ExtractAllProductsToXML("d://Offers/" + supplier.ShortName + ".xml", supplier.Id);
                }
                

            }
        }

        private void btUploadNewOffers_Click(object sender, RoutedEventArgs e)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
               FTPManager.CheckAndUpdateSuppliersOffers();
            }
        }
    }
}
