const formatadorMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

const formatadorQuantidade = new Intl.NumberFormat('pt-BR', {
  minimumFractionDigits: 0,
  maximumFractionDigits: 4,
});

export const formatarMoeda = (valor: number): string => formatadorMoeda.format(valor);

export const formatarQuantidade = (valor: number): string => formatadorQuantidade.format(valor);

/** A API trafega datas como `YYYY-MM-DD` (DateOnly do backend). A tela mostra `DD/MM/YYYY`. */
export function formatarData(iso: string): string {
  const [ano, mes, dia] = iso.split('-');
  return `${dia}/${mes}/${ano}`;
}
