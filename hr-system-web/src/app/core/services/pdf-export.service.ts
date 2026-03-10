import { Injectable } from '@angular/core';
@Injectable({ providedIn: 'root' })
export class PdfExportService {
  async exportTable(
    title: string,
    filename: string,
    columns: string[],
    rows: Array<Array<string | number>>
  ) {
    const [{ jsPDF }, autoTableModule] = await Promise.all([import('jspdf'), import('jspdf-autotable')]);
    const autoTable = autoTableModule.default;
    const doc = new jsPDF({
      orientation: 'landscape',
      unit: 'pt',
      format: 'a4'
    });

    doc.setFontSize(16);
    doc.text(title, 40, 42);

    autoTable(doc, {
      head: [columns],
      body: rows,
      startY: 60,
      margin: { left: 40, right: 40 },
      styles: {
        fontSize: 9,
        cellPadding: 6,
        overflow: 'linebreak'
      },
      headStyles: {
        fillColor: [37, 99, 235]
      }
    });

    doc.save(filename);
  }
}
