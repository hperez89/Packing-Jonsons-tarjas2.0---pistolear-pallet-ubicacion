using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using iTextSharp.text.pdf;

namespace tarjas2._0
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public static MySqlConnection conectar, conectar2;
        //VARIABLE QUE ME ALMACENA CUANTAS CAJAS HAY
        public static int contador = 1;
        //VARIABLE QUE ME DICE CUAL ES EL ULTIMO ID INGRESADO EN EL DATAGRIDVIEW1
        public static int contador2 = 1;
        //VARIABLE QUE ME DICE EL NUMERO ASCII DE LA TECLA PRESIONADA EN LA CASILLA DE INGRESO DE CAJAS
        //int enter2 = 0;
        //VARIABLE QUE ME DICE EL NUMERO ASCII DE LA TECLA PRESIONADA EN LA CASILLA DEL CODIGO PALLET
        int enter3 = 0;
        //VARIABLE QUE ALMACENA EL CODIGO DEL PALLET CUANDO SE ENCUENTRA YA DEFINIDO
        //PARA NO CREAR OTRO CODIGO SI ESTE YA SE IMPRIMIO
        public static string codigo_pallet_def = "";
        //VARIABLE QUE GUARDA LA CATEGORIA DEL PALLET
        public static string CatPallet = "";
        //VARIABLE QUE GUARDA LA EXPORTADORA DEL PALLET
        public static string ExpPallet = "";
        //VARIABLE QUE GUARDA EL ENBALAJE DEL PALLET
        public static string EmbPallet = "";
        //VARIABLE QUE GUARDA EL TIPO DE FRIO OCUPADO
        public static string FrioPallet = "";
        //VARIABLE QUE GUARDA LA ESPECIE DEL PALLET
        public static string EspPallet = "";
        //VARIABLE QUE ALMACENA LA ULTIMA FILA IMPRESA DEL DETALLE DE PALLET, SIRVE PARA SEGUIR IMPRIMIENDO
        //EN MAS PAGINAS SI ES NECESARIO
        public static int ultimafila = 0;
        //VARIABLE QUE ALMACENA SI ES NECESARIO SEGUIR IMPRIMIENDO EN MAS PÁGINAS
        public static bool maspaginas = false;
        //VARIABLE QUE ALMACENA EL TOTAL DE FILAS DEL DETALLE A IMPRIMIR
        public static int total = 0;
        //VARIABLE QUE ALMACENA EL TOTAL DE FILAS QUE SE IMPRIMEN POR PÁGINA
        public static int porpagina = 40;
        //VARIABLE QUE ALMACENA EL TOTAL DE FILAS QUE SE IMPRIMEN POR PÁGINA EN RESUMEN TARJA
        public static int porpaginaTarja = 18;
        //VARIABLE QUE ALMACENA EL PRODUCTOR DE LAS ETIQUETAS
        public static string productor_etiq = "";
        //VARIABLE QUE ALMACENA LA ESPECIE DE LAS ETIQUETAS
        public static string especie_etiq = "";
        //VARIABLE QUE ALMACENA LA VARIEDAD DE LAS ETIQUETAS
        public static string variedad_etiq = "";
        //VARIABLE QUE ALMACENA EL CALIBRE DE LAS ETIQUETAS
        public static string calibre_etiq = "";
        //VARIABLE QUE ALMACENA EL CODIGO DEL PALLET QUE SE DEVUELVE A BUSCARLO POR EL CODIGO DE UNA CAJA
        public static string codigo_pallet_desde_caja = "";
        public static int fila_caja = -1;
		public static int hora_apaga = 0;
		public static string essaldo, nro_folio, id_productor, id_variedad, id_calibre, cant_cajas, fecha, id_packing, packing, id_embalaje;
		public Form1()
        {
            InitializeComponent();
        }
        //public static void ObtenerConexion()
        //{
        //    conectar = new MySqlConnection("server=localhost; database=etiquetado_system; Uid=root; pwd=spsi2018;");
        //    conectar.Open();
        //}
        public static void ObtenerConexion2()
        {
            conectar2 = new MySqlConnection("server=192.168.1.84; database=bd_planta; Uid=johnson; pwd=1234;");
            conectar2.Open();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
			HistorialTxt.Text = "";
			//HistorialTxt.Text = "Ingresado G3500010001\r\n";
			//string aux = HistorialTxt.Text;
			//HistorialTxt.Text = "";
			//HistorialTxt.Text += "Ingresado T3500030002\r\n" + aux;
			CargaRecibidores();
			CargaMercado();
			CargaEmbalajes();
			CargaVariedades();
			//timer2.Start();
			//Pallet_despachados();
			//TurnoBox.Text = "1";
			//         PalletTipo.Text = "Puro";
			CamaraTxt.Text = "CAMARA 1";
			MercadoTxt.Text = "USA";
			AlturaBox.Text = "2,4 Mts.";
			PalletTipoTxt.Text = "PALLET TACO";
			RecibidorTxt.Text = "SIN RECIBIDOR";
			NroProcesoTxt.Text = "0";
			InspeccionTxt.Text = "NO";
			FumigacionTxt.Text = "NO";
			CodigoPallet.Focus();
			//TimerApagado.Start();
		}
        static int Asc(char c)
        {
            int converted = c;
            if (converted >= 0x80)
            {
                byte[] buffer = new byte[2];
                // if the resulting conversion is 1 byte in length, just use the value
                if (System.Text.Encoding.Default.GetBytes(new char[] { c }, 0, 1, buffer, 0) == 1)
                {
                    converted = buffer[0];
                }
                else
                {
                    // byte swap bytes 1 and 2;
                    converted = buffer[0] << 16 | buffer[1];
                }
            }
            return converted;
        }
        private void CodigoCajaTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            //enter2 = Asc(e.KeyChar);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            
            //PREGUNTO SI SE HIZO UN ENTER EN EL INGRESO DE UN CODIGO DE PALLET O TARJA
            if (enter3.Equals(13))
            {
                //agrego info del pallet
                enter3 = 0;
				//InfoPallet(CodigoPallet.Text);
				Rescatar_info(CodigoInteligenteTxt.Text);
				MetroTile1_Click(sender, e);
				CodigoInteligenteTxt.Text = "";
				//agrego informacion del folio del pallet ingresado (sea por Base de Datos o por codigo inteligente: 'crear funcion)
			}
            //NumeroCajas.Text = Convert.ToString(contador - 1);
        }

		private void Rescatar_info(string codigointeligente)
		{
			InspeccionTxt.Text = "NO";
			FumigacionTxt.Text = "NO";
			
			essaldo = codigointeligente.Substring(1, 1);
			if (essaldo.Equals("0")) { CompletoSaldoTxt.Text = "COMPLETO"; nro_folio = "G35000"; } else { CompletoSaldoTxt.Text = "SALDO"; nro_folio = "T35000"; }
			id_productor = codigointeligente.Substring(6,1);
			if (id_productor.Equals("1")) { ProductorTxt.Text = "FUNDO EL CARDAL"; }
			if (id_productor.Equals("2")) { ProductorTxt.Text = "FUNDO EL CARMEN"; }
			if (id_productor.Equals("3")) { ProductorTxt.Text = "FUNDO LA ESTRELLA"; }
			if (id_productor.Equals("4")) { ProductorTxt.Text = "FUNDO SAN DAMIAN"; }
			if (id_productor.Equals("5")) { ProductorTxt.Text = "FUNDO SAN LUIS"; }
			if (id_productor.Equals("6")) { ProductorTxt.Text = "FUNDO SANTA BLANCA"; }
			if (id_productor.Equals("7")) { ProductorTxt.Text = "FUNDO TIERRA CHILENA"; }
			if (id_productor.Equals("8")) { ProductorTxt.Text = "FUNDO TRES VERTIENTES"; }
			
			id_variedad = codigointeligente.Substring(7, 2);
			VariedadTxt.Text = ObtenerVariedad(id_variedad);
			//if (id_variedad.Equals("0")) { VariedadTxt.Text = "AUTUMN ROYAL"; }
			//if (id_variedad.Equals("1")) { VariedadTxt.Text = "CRIMSON SEEDLESS"; }
			//if (id_variedad.Equals("2")) { VariedadTxt.Text = "FLAME SEED LESS"; }
			//if (id_variedad.Equals("3")) { VariedadTxt.Text = "PRINCESS SEEDLESS"; }
			//if (id_variedad.Equals("4")) { VariedadTxt.Text = "RED GLOBE"; }
			//if (id_variedad.Equals("5")) { VariedadTxt.Text = "SUGRAONE"; }
			//if (id_variedad.Equals("6")) { VariedadTxt.Text = "SUGRAONE / FLAME"; }
			//if (id_variedad.Equals("7")) { VariedadTxt.Text = "THOMPSON SEEDLESS"; }
			//if (id_variedad.Equals("8")) { VariedadTxt.Text = "THOMPSON/CRIMSON"; }
			id_calibre = codigointeligente.Substring(9, 2);
			CalibreTxt.Text = ObtenerCalibre(Convert.ToInt32(id_calibre).ToString());
			//if (id_calibre.Equals("03")) { CalibreTxt.Text = "500"; }
			//if (id_calibre.Equals("04")) { CalibreTxt.Text = "700"; }
			//if (id_calibre.Equals("05")) { CalibreTxt.Text = "900"; }
			//if (id_calibre.Equals("06")) { CalibreTxt.Text = "1000"; }
			//if (id_calibre.Equals("09")) { CalibreTxt.Text = "L"; }
			//if (id_calibre.Equals("16")) { CalibreTxt.Text = "XXL"; }
			//if (id_calibre.Equals("21")) { CalibreTxt.Text = "18MM"; }
			//if (id_calibre.Equals("22")) { CalibreTxt.Text = "16MM"; }
			//if (id_calibre.Equals("35")) { CalibreTxt.Text = "17.5MM"; }
			//if (id_calibre.Equals("37")) { CalibreTxt.Text = "17 MM"; }
			//if (id_calibre.Equals("44")) { CalibreTxt.Text = "22MM"; }
			//if (id_calibre.Equals("45")) { CalibreTxt.Text = "MA"; }
			//if (id_calibre.Equals("47")) { CalibreTxt.Text = "700A"; }
			//if (id_calibre.Equals("48")) { CalibreTxt.Text = "LA"; }
			//if (id_calibre.Equals("49")) { CalibreTxt.Text = "XL"; }
			cant_cajas = codigointeligente.Substring(11, 3);
			CajasPalletTxt.Text = cant_cajas;
			fecha = codigointeligente.Substring(14, 4) + "-" + codigointeligente.Substring(18, 2) + "-" + codigointeligente.Substring(20, 2);
			FechaTxt.Text = codigointeligente.Substring(20, 2) + "-" + codigointeligente.Substring(18, 2) + "-" + codigointeligente.Substring(14, 4);
			id_packing = codigointeligente.Substring(22, 1);
			if (id_packing.Equals("1")) { packing = "FUNDO EL CARDAL"; }
			if (id_packing.Equals("2")) { packing = "FUNDO SANTA BLANCA"; }
			if (id_packing.Equals("3")) { packing = "FUNDO SAN LUIS LO MOSCOSO"; }
			if (id_packing.Equals("4")) { packing = "FUNDO TIERRA CHILENA"; }
			if (id_packing.Equals("5")) { packing = "JOHNSON FRUITS S.A."; }
			nro_folio += id_packing + codigointeligente.Substring(2, 4);
			CodigoPallet.Text = nro_folio;
			id_embalaje = codigointeligente.Substring(23, 3);
			EmbalajeTxt.Text = ObtenerEmbalaje(Convert.ToInt32(id_embalaje).ToString());
			Double kilos_netos, kilos_brutos;
			kilos_netos = Convert.ToDouble(cant_cajas) * Obtener_kilos_netos(EmbalajeTxt.Text);
			kilos_brutos = Convert.ToDouble(cant_cajas) * Obtener_kilos_brutos(EmbalajeTxt.Text);
			KGBrutosTxt.Text = kilos_brutos.ToString();
			KGNetosTxt.Text = kilos_netos.ToString();
			CategoriaTxt.Text = ObtenerCategoriaEmbalaje(Convert.ToInt32(id_embalaje).ToString());
		}
        
        private void MetroButton3_Click(object sender, EventArgs e)
        {
            
        }

        private void MetroGrid1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }
     
        private void Print_PrintPage(Object sender, PrintPageEventArgs e)
        {
   
        }
		
        private void CodigoPallet_TextChanged(object sender, EventArgs e)
        {
            
        }
       
        private void MetroButton1_Click(object sender, EventArgs e)
        {

		}
        

        private void MetroButton2_Click(object sender, EventArgs e)
        {

        }

        private void MetroTile1_Click(object sender, EventArgs e)
        {
			try
			{
				if (CodigoPallet.Text.Length > 0 && CompletoSaldoTxt.Text.Length > 0 && AlturaBox.Text.Length > 0 && CalibreTxt.Text.Length > 0
					&& CajasPalletTxt.Text.Length > 0 && MercadoTxt.Text.Length > 0 && PalletTipoTxt.Text.Length > 0 && ProductorTxt.Text.Length > 0
					&& VariedadTxt.Text.Length > 0 && InspeccionTxt.Text.Length > 0 && KGBrutosTxt.Text.Length > 0 && KGNetosTxt.Text.Length > 0
					&& CamaraTxt.Text.Length > 0 && ResponsableTxt.Text.Length > 0 && EmbalajeTxt.Text.Length > 0 && CategoriaTxt.Text.Length > 0
					&& FumigacionTxt.Text.Length > 0 && RecibidorTxt.Text.Length > 0 && NroProcesoTxt.Text.Length > 0 && FechaTxt.Text.Length > 0)
				{
					MySqlConnection conectarx;
					conectarx = new MySqlConnection("server=192.168.1.84; database=bd_planta; Uid=johnson; pwd=1234;");
					conectarx.Open();
					string auxpallet = "COMPLETO";
					if (CompletoSaldoTxt.Text.Equals("SALDO")) { auxpallet = "INCOMPLETO"; }
					MySqlCommand _comando2 = new MySqlCommand(String.Format(
					"INSERT INTO bd_planta.pallet(folio_pallet, tipo_pallet, kg_bruto, kg_neto, cajas_por_pallet, camara_frio, nro_de_ingreso, fecha_ingreso, usuario_responsable," +
					"kg_bruto_total, kg_neto_total, estado, inspeccion, mercado, recibidor, exportador, variedad, embalaje, especie, categoria, altura, tipo, kg_neto_total_produccion," +
					"fumigacion, marca, mitigacion) VALUES('" + CodigoPallet.Text + "', '" + auxpallet + "', '" + KGBrutosTxt.Text + "', '" + KGNetosTxt.Text + "', '" + CajasPalletTxt.Text + "', '" + CamaraTxt.Text + "', NULL, '" + fecha + "', '17992752-7'," +
					"'" + KGBrutosTxt.Text + "', '" + KGNetosTxt.Text + "', 'ALMACENADO', '" + InspeccionTxt.Text + "', '" + MercadoTxt.Text + "', '" + RecibidorTxt.Text + "', 'AGRICOLA SAN LUIS DE YAQUIL', '" + VariedadTxt.Text + "', '" + EmbalajeTxt.Text + "' , 'UVA DE MESA', '" + ObtenerCategoriaEmbalaje(id_embalaje) + "', '" + AlturaBox.Text.ToUpper() + "', '" + PalletTipoTxt.Text + "', NULL," +
					"'" + FumigacionTxt.Text + "', 'JOHNSON', 'NO')"), conectarx);
					MySqlDataReader _reader2 = _comando2.ExecuteReader();
					conectarx.Close();

					MySqlConnection conectarx2;
					conectarx2 = new MySqlConnection("server=192.168.1.84; database=bd_planta; Uid=johnson; pwd=1234;");
					conectarx2.Open();
					string color = ObtenerColor(VariedadTxt.Text);
					MySqlCommand _comando3 = new MySqlCommand(String.Format(
					"INSERT INTO bd_planta.detalle_pallet(nro_folio_pallet_detalle, nro_folio_pallet, nro_proceso, csg, kg_bruto_detalle, " +
					"kg_neto_detalle, fecha_embalaje, variedad, calibre, color, csp, cajas, especie, embalaje, idg, idp, recibidor, " +
					"peso_bruto_detalle, peso_neto_detalle, etiqueta, peso_neto_detalle_produccion, lote) VALUES(NULL, " +
					"'" + CodigoPallet.Text + "', '" + NroProcesoTxt.Text + "', '" + ObtenerCSG(ProductorTxt.Text) + "', '" + Obtener_kilos_brutos(EmbalajeTxt.Text).ToString() + "', '" + Obtener_kilos_netos(EmbalajeTxt.Text).ToString() + "', '" + fecha + "', '" + VariedadTxt.Text + "', '" + CalibreTxt.Text + "', " +
					"'" + color + "', '" + ObtenerCSP(packing) + "', '" + CajasPalletTxt.Text + "', 'UVA DE MESA', '" + EmbalajeTxt.Text + "', '" + ObtenerIDG(ProductorTxt.Text) + "', '" + ObtenerIDP(packing) + "', '" + RecibidorTxt.Text + "', '" + KGBrutosTxt.Text + "', '" + KGNetosTxt.Text + "', 'JOHNSON', NULL, NULL)"), conectarx2);
					MySqlDataReader _reader3 = _comando3.ExecuteReader();

					conectarx2.Close();
					//MetroFramework.MetroMessageBox.Show(this, "Almacenado con exito.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
					string aux = HistorialTxt.Text;
					HistorialTxt.Text = "";
					HistorialTxt.Text += "Ingresado " + CodigoPallet.Text + "\r\n" + aux;
				}
				else
				{
					MetroFramework.MetroMessageBox.Show(this, "Favor rellene todos los campos.", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

			}
			catch (Exception)
			{
				string aux = HistorialTxt.Text;
				HistorialTxt.Text = "";
				HistorialTxt.Text += "Error al guardar " + CodigoPallet.Text + "\r\n" + aux;
				//MetroFramework.MetroMessageBox.Show(this, "Hubo un error al guardar la información, avise a informática para saber si hay problemas en la red. Error L226", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			
		}
		public string Obtener_Categoria(string embalaje)
		{
			ObtenerConexion2();
			string categoria = "";
			//CONECTA A LA BD
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT embalaje.categoria FROM embalaje WHERE embalaje.abreviacion_embalaje = '" + embalaje + "'"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			if (_reader.Read())
			{
				categoria = _reader.GetString(0);
			}
			conectar2.Close();
			return categoria;
		}
		
		public string ObtenerIDG(string productor)
		{
			string idg = "";
			ObtenerConexion2();
			//CONECTA A LA BD    GUARDAR DETALLE EN DETALLE_PALLET ERP JOHNSON
			MySqlCommand _comando3 = new MySqlCommand(String.Format(
			"SELECT productor.idg FROM productor WHERE productor.nombre_fundo = '" + productor + "'"), conectar2);
			MySqlDataReader _reader3 = _comando3.ExecuteReader();
			if (_reader3.Read())
			{
				idg = _reader3.GetString(0);
			}
			conectar2.Close();
			return idg;
		}
		
		private void MetroButton4_Click(object sender, EventArgs e)
        {
           
        }
	
		public Double Obtener_kilos_netos(string embalaje)
		{
			//SELECT etiqueta.embalaje FROM etiqueta, tarja WHERE tarja.codigo_tarja = etiqueta.codigo_tarja AND tarja.codigo_tarja = 'G3500050003' LIMIT 1
			Double kilosnetos = 0;
			ObtenerConexion2();
			//CONECTA A LA BD    GUARDAR DETALLE EN DETALLE_PALLET ERP JOHNSON
			MySqlCommand _comando3 = new MySqlCommand(String.Format(
			"SELECT embalaje.peso_neto FROM embalaje WHERE embalaje.abreviacion_embalaje = '" + embalaje + "'"), conectar2);
			MySqlDataReader _reader3 = _comando3.ExecuteReader();
			if (_reader3.Read())
			{
				kilosnetos = Convert.ToDouble(_reader3.GetString(0));
			}
			conectar2.Close();
			return kilosnetos;
		}
		public Double Obtener_kilos_brutos(string embalaje)
		{
			//
			Double kilosbrutos = 0;
			ObtenerConexion2();
			//CONECTA A LA BD    GUARDAR DETALLE EN DETALLE_PALLET ERP JOHNSON
			MySqlCommand _comando3 = new MySqlCommand(String.Format(
			"SELECT embalaje.peso_bruto FROM embalaje WHERE embalaje.abreviacion_embalaje = '" + embalaje + "'"), conectar2);
			MySqlDataReader _reader3 = _comando3.ExecuteReader();
			if (_reader3.Read())
			{
				kilosbrutos = Convert.ToDouble(_reader3.GetString(0));
			}
			conectar2.Close();
			return kilosbrutos;
		}
		private void CodigoPallet_KeyPress(object sender, KeyPressEventArgs e)
        {
            //enter3 = Asc(e.KeyChar);
        }

        public string ObtenerCSG(string productor)
        {
            string CSG = "";
			ObtenerConexion2();
            MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT productor.csg FROM productor WHERE productor.nombre_fundo = '" + productor + "'"), conectar2);
            MySqlDataReader _reader = _comando.ExecuteReader();
            while (_reader.Read())
            {
                CSG = _reader.GetString(0);
            }
            conectar2.Close();

            return CSG;
        }

        
		public void CargaRecibidores()
		{
			RecibidorTxt.Items.Clear();
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT consignatario.nombre_consignatario FROM bd_planta.consignatario ORDER BY consignatario.nombre_consignatario ASC"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				RecibidorTxt.Items.Add(_reader.GetString(0));
			}
			conectar2.Close();
			
		}

		private void CleanButton_Click(object sender, EventArgs e)
		{
			HistorialTxt.Text = "";
		}

		private void ProductorTxt_SelectedIndexChanged(object sender, EventArgs e)
		{

		}
		public void CargaVariedades()
		{
			VariedadTxt.Items.Clear();
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT variedad.nombre_variedad FROM variedad WHERE variedad.nombre_especie = 'UVA DE MESA' ORDER BY variedad.nombre_variedad ASC"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				VariedadTxt.Items.Add(_reader.GetString(0));
			}
			conectar2.Close();
		}
		public void CargaMercado()
		{
			MercadoTxt.Items.Clear();
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT mercado.nombre_mercado FROM mercado ORDER BY mercado.nombre_mercado ASC"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				MercadoTxt.Items.Add(_reader.GetString(0));
			}
			conectar2.Close();
		}
		
		public void CargaEmbalajes()
		{
			EmbalajeTxt.Items.Clear();
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT embalaje.abreviacion_embalaje FROM embalaje WHERE embalaje.nombre_especie LIKE 'UVA%' ORDER BY embalaje.abreviacion_embalaje ASC"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				EmbalajeTxt.Items.Add(_reader.GetString(0));
			}
			conectar2.Close();
		}
		public string ObtenerEmbalaje(string id)
		{
			string embalaje = "";
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT embalaje.abreviacion_embalaje FROM embalaje WHERE embalaje.codigo_embalaje = '"+ id + "'"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				embalaje = _reader.GetString(0);
			}
			conectar2.Close();
			return embalaje;
		}
		public string ObtenerCalibre(string id)
		{
			string calibre = "";
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT calibre.abreviacion_calibre FROM calibre WHERE calibre.codigo_calibre = '" + id + "'"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				calibre = _reader.GetString(0);
			}
			conectar2.Close();
			return calibre;
		}
		public string ObtenerVariedad(string id)
		{
			string variedad = "";
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT variedad.nombre_variedad FROM variedad WHERE variedad.codigo_variedad = '" + id + "'"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				variedad = _reader.GetString(0);
			}
			conectar2.Close();
			return variedad;
		}
		public string ObtenerCategoriaEmbalaje(string id_embalaje)
		{
			string categoria = "";
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT embalaje.categoria FROM embalaje WHERE embalaje.codigo_embalaje = '" + id_embalaje + "'"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				categoria = _reader.GetString(0);
			}
			conectar2.Close();
			return categoria;
		}
		public string ObtenerColor(string variedad)
		{
			string color = "";
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT variedad.color FROM variedad WHERE variedad.nombre_variedad = '" + variedad + "'"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				color = _reader.GetString(0);
			}
			conectar2.Close();
			return color;
		}
		public string ObtenerCSP(string nombre_packing)
		{
			string csp = "";
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT packing.csp FROM bd_planta.packing WHERE packing.nombre_packing = '" + nombre_packing + "'"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				csp = _reader.GetString(0);
			}
			conectar2.Close();
			return csp;
		}
		public string ObtenerIDP(string nombre_packing)
		{
			string idp = "";
			ObtenerConexion2();
			MySqlCommand _comando = new MySqlCommand(String.Format(
			"SELECT packing.idp FROM bd_planta.packing WHERE packing.nombre_packing = '" + nombre_packing + "'"), conectar2);
			MySqlDataReader _reader = _comando.ExecuteReader();
			while (_reader.Read())
			{
				idp = _reader.GetString(0);
			}
			conectar2.Close();
			return idp;
		}
		private void Timer2_Tick(object sender, EventArgs e)
        {
                        
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            //timer2.Stop();
        }

		private void TimerApagado_Tick(object sender, EventArgs e)
		{
			
		}

		private void MetroTextBox1_KeyPress(object sender, KeyPressEventArgs e)
		{
			enter3 = Asc(e.KeyChar);
		}

		
    }
}
