import axios, { AxiosError } from 'axios';

/**
 * Instância única do Axios. Todo acesso ao servidor passa por aqui — nada de `fetch` solto pelas
 * telas, senão a URL base e o tratamento de erro viram 15 cópias diferentes.
 */
const cliente = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api/v1',
  headers: { 'Content-Type': 'application/json' },
});

/** Formato de erro que o backend devolve em 422 (regra de negócio violada). */
interface ErroDaApi {
  codigo?: string;
  mensagem?: string;
}

/**
 * Extrai a mensagem que o usuário deve ver.
 *
 * Um 422 sempre traz uma mensagem escrita para humanos — é ela que aparece. Qualquer outra falha
 * cai numa mensagem genérica: expor o texto cru de um erro de servidor não ajuda ninguém.
 */
export function mensagemDoErro(erro: unknown, padrao = 'Não foi possível concluir a operação.'): string {
  const axiosErro = erro as AxiosError<ErroDaApi>;

  if (axiosErro?.response?.status === 422 && axiosErro.response.data?.mensagem) {
    return axiosErro.response.data.mensagem;
  }

  if (axiosErro?.code === 'ERR_NETWORK') {
    return 'Não foi possível falar com o servidor. A API está rodando?';
  }

  return padrao;
}

export default cliente;
